using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Customizations;
using Alma.Workflows.Enums;
using Alma.Workflows.Models.Activities;
using Alma.Workflows.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Text;
using System.Xml.Serialization;

namespace Alma.Workflows.Activities.Integration
{
    [Activity(
        Namespace = "Alma.Workflows",
        Category = "Integração",
        DisplayName = "Requisição HTTP",
        Description = "Faz uma requisição HTTP.")]
    [ActivityCustomization(Icon = FlowIcons.Http, BorderColor = FlowColors.Integration)]
    public class HttpRequestActivity : Core.Activities.Base.Activity
    {
        #region Ports

        [Port(DisplayName = "Entrada", Type = PortType.Input)]
        public Port Input { get; set; } = default!;

        [Port(DisplayName = "Sucesso", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Success)]
        public Port Success { get; set; } = default!;

        [Port(DisplayName = "Falha", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Error)]
        public Port Fail { get; set; } = default!;

        [Port(DisplayName = "Erro", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Error)]
        public Port Error { get; set; } = default!;

        #endregion

        #region Parameters

        [ActivityParameter(DisplayName = "Método", DisplayValue = "{{value}}:")]
        public Parameter<Enums.HttpMethod>? Method { get; set; }

        [ActivityParameter(DisplayName = "Url", DisplayValue = "{{value}}")]
        public Parameter<string>? Url { get; set; }

        [ActivityParameter(DisplayName = "Cabeçalhos")]
        public Parameter<Dictionary<string, string>>? Header { get; set; }

        [ActivityParameter(DisplayName = "Tipo de conteúdo")]
        public Parameter<HttpContentType>? ContentType { get; set; }

        [ActivityParameter(DisplayName = "Corpo", AutoGrow = true, Lines = 3, MaxLines = 8)]
        public Parameter<string>? Body { get; set; }

        #endregion

        public override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var httpClientFactory = context.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();

            try
            {
                var request = new HttpRequestMessage(new System.Net.Http.HttpMethod(Method?.GetValue(context).ToString() ?? "Get"), Url?.GetValue(context));

                if (Header?.GetValue(context) != null)
                {
                    foreach (var header in Header.GetValue(context))
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                if (Body?.GetValue(context) != null)
                {
                    if (ContentType?.GetValue(context) is null || ContentType.GetValue(context) == HttpContentType.Json)
                    {
                        request.Content = new StringContent(Body.GetValue(context), Encoding.UTF8, "application/json");
                    }
                    else if (ContentType.GetValue(context) == HttpContentType.FormUrlEncoded)
                    {
                        request.Content = HttpRequestUtils.ConvertToFormUrlEncodedContent(Body.GetValue(context));
                    }
                    else if (ContentType.GetValue(context) == HttpContentType.Xml)
                    {
                        request.Content = new StringContent(Body.GetValue(context), Encoding.UTF8, "application/xml");
                    }
                    else
                    {
                        request.Content = new StringContent(Body.GetValue(context), Encoding.UTF8);
                    }
                }

                var stopwatch = Stopwatch.StartNew();

                var response = await httpClient.SendAsync(request);

                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    var result = new HttpRequestActivityResponse
                    {
                        StatusCode = (int)response.StatusCode,
                        ElapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds,
                        Content = await response.Content.ReadAsStringAsync(),
                        Body = ConvertResponseToObject(await response.Content.ReadAsStringAsync(), response.Content.Headers.ContentType?.MediaType)
                    };

                    Success.Execute(result);
                }
                else
                {
                    var result = new HttpRequestActivityResponse
                    {
                        StatusCode = (int)response.StatusCode,
                        Content = await response.Content.ReadAsStringAsync(),
                        Body = ConvertResponseToObject(await response.Content.ReadAsStringAsync(), response.Content.Headers.ContentType?.MediaType)
                    };

                    Fail.Execute(result);
                }
            }
            catch (Exception ex)
            {
                Error.Execute(ex.Message);
            }
        }

        public object? ConvertResponseToObject(string response, string? contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return response;

            if (string.IsNullOrEmpty(response))
                return null;

            if (contentType.Contains("application/json"))
            {
                return DotLiquidUtils.ConvertJsonToHash(response);
            }

            if (contentType.Contains("application/xml"))
            {
                var serializer = new XmlSerializer(typeof(object));

                using (var reader = new StringReader(response))
                {
                    try
                    {
                        return serializer.Deserialize(reader);
                    }
                    catch
                    {
                        return response;
                    }
                }
            }

            return response;
        }
    }
}