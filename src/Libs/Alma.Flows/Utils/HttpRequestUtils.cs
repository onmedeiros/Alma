namespace Alma.Flows.Utils
{
    public static class HttpRequestUtils
    {
        public static FormUrlEncodedContent ConvertToFormUrlEncodedContent(string input)
        {
            var keyValuePairs = new Dictionary<string, string>();

            var lines = input.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var keyValue = line.Split(new[] { '=' }, 2);
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0].Trim();
                    string value = keyValue[1].Trim();

                    keyValuePairs[key] = value;
                }
            }
            return new FormUrlEncodedContent(keyValuePairs);
        }
    }
}
