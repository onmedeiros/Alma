using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Customizations;
using Alma.Workflows.Models.Activities;
using NCalc;
using Org.BouncyCastle.Asn1.X509;
using System.Collections;

namespace Alma.Workflows.Activities.Flow
{
    [Activity(
        Namespace = "Alma.Workflows",
        Category = "Fluxo",
        DisplayName = "Loop",
        Description = "Executa um loop iterativo sobre uma coleção, contagem ou condição.")]
    [ActivityCustomization(Icon = FlowIcons.Action, BorderColor = FlowColors.Flow)]
    public class LoopActivity : Activity
    {
        #region Ports

        [Port(DisplayName = "Entrada", Type = PortType.Input)]
        public Port Input { get; set; } = default!;

        [Port(DisplayName = "Corpo do Loop", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Flow)]
        public Port Body { get; set; } = default!;

        [Port(DisplayName = "Completo", Type = PortType.Input)]
        public Port BodyComplete { get; set; } = default!;

        [Port(DisplayName = "Concluído", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Success)]
        public Port Done { get; set; } = default!;

        [Port(DisplayName = "Erro", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Error)]
        public Port Error { get; set; } = default!;

        #endregion

        #region Parameters

        [ActivityParameter(DisplayName = "Tipo", DisplayValue = "<b>Tipo:</b> {{value}}")]
        public Parameter<LoopType>? Type { get; set; }

        [ActivityParameter(DisplayName = "Contador inicial", DisplayValue = "<b>Início:</b> {{value}}")]
        public Parameter<int>? StartCount { get; set; }

        [ActivityParameter(DisplayName = "Contador final", DisplayValue = "<b>Fim:</b> {{value}}")]
        public Parameter<int>? EndCount { get; set; }

        [ActivityParameter(DisplayName = "Coleção (JSON ou nome da variável)", DisplayValue = "<b>Coleção:</b> {{value}}", AutoGrow = true, MaxLines = 4)]
        public Parameter<string>? Collection { get; set; }

        [ActivityParameter(DisplayName = "Condição", DisplayValue = "<b>Condição:</b> {{value}}", AutoGrow = true, MaxLines = 4)]
        public Parameter<string>? WhileCondition { get; set; }

        [ActivityParameter(DisplayName = "Nome da variável do item atual", DisplayValue = "<b>Variável:</b> {{value}}")]
        public Parameter<string>? CurrentItemVariableName { get; set; }

        [ActivityParameter(DisplayName = "Nome da variável do índice", DisplayValue = "<b>Índice:</b> {{value}}")]
        public Parameter<string>? IndexVariableName { get; set; }

        [ActivityParameter(DisplayName = "Máximo de iterações (proteção)", DisplayValue = "<b>Máx iterações:</b> {{value}}")]
        public Parameter<int>? MaxIterations { get; set; }

        #endregion

        public override IsReadyResult IsReadyToExecute(ActivityExecutionContext context)
        {
            // Se o loop já foi completado, não executar novamente
            var isLoopComplete = context.State.Memory.Get<bool?>(Id, "isLoopComplete") ?? false;

            if (isLoopComplete)
            {
                return IsReadyResult.NotReady("Loop already done.");
            }

            return IsReadyResult.Ready();
        }

        public override void Execute(ActivityExecutionContext context)
        {
            try
            {
                var loopType = Type?.GetValue(context) ?? LoopType.Count;
                var phase = context.State.Memory.Get<string>(Id, "phase");
                var isLoopInitialized = context.State.Memory.Get<bool>(Id, "isLoopInitialized");
                var currentIndex = context.State.Memory.Get<int>(Id, "currentIndex");

                context.State.Logs.Add($"Loop executando na fase: {phase}, Iteração: {currentIndex}", Enums.LogSeverity.Debug);

                // Phase 1: Initialization
                if (phase == LoopConstants.PhaseStart || !isLoopInitialized)
                {
                    // Initilize loop if it's not initialized
                    if (!isLoopInitialized)
                    {
                        InitializeLoop(context, loopType);
                        context.State.Memory.Set(Id, "isLoopInitialized", true);
                    }

                    // Verify if should continue
                    if (ShouldContinueLoop(context, loopType))
                    {
                        SetLoopVariables(context, loopType);

                        // Change to waiting body phase
                        context.State.Memory.Set(Id, "phase", LoopConstants.PhaseWaitingBody);

                        // Execute loop body
                        Body.Execute();
                    }
                    else
                    {
                        // Loop complete from start
                        FinalizeLoop(context);
                        Done.Execute();
                    }
                }
                // Fase 2: Corpo do loop foi completado
                else if (phase == LoopConstants.PhaseBodyCompleted)
                {
                    // Incrementa o índice
                    IncrementLoop(context);

                    // Reseta a fase para início
                    context.State.Memory.Set(Id, "phase", LoopConstants.PhaseStart);

                    // Verifica se deve continuar
                    if (ShouldContinueLoop(context, loopType))
                    {
                        SetLoopVariables(context, loopType);

                        // Muda para a fase de aguardar corpo completo
                        context.State.Memory.Set(Id, "phase", LoopConstants.PhaseWaitingBody);

                        // Executa o corpo do loop novamente
                        Body.Execute();
                    }
                    else
                    {
                        // Loop completo
                        FinalizeLoop(context);
                        Done.Execute();
                    }
                }
                // Fase 3: Aguardando o corpo ser completado
                else if (phase == LoopConstants.PhaseWaitingBody)
                {
                    // Esta fase não executa nada, apenas aguarda o BodyComplete
                    // Quando o BodyComplete for executado, a próxima execução será na fase BodyCompleted
                }
            }
            catch (Exception ex)
            {
                context.State.Logs.Add($"Erro no loop: {ex.Message}", Enums.LogSeverity.Error);
                FinalizeLoop(context);
                Error.Execute(ex.Message);
            }
        }

        private void InitializeLoop(ActivityExecutionContext context, LoopType loopType)
        {
            context.State.Memory.Set(Id, "currentIndex", 0);
            context.State.Memory.Set(Id, "isLoopComplete", false);
            context.State.Memory.Set(Id, "phase", LoopConstants.PhaseStart);

            switch (loopType)
            {
                case LoopType.Count:
                    var start = StartCount?.GetValue(context) ?? 0;
                    var end = EndCount?.GetValue(context) ?? 0;

                    context.State.Memory.Set(Id, "totalInteractions", Math.Max(0, end - start + 1));
                    context.State.Memory.Set(Id, "currentIndex", start);
                    context.State.Logs.Add($"Loop de contagem inicializado: {start} até {end}", Enums.LogSeverity.Information);

                    break;

                case LoopType.Collection:
                    var collection = GetCollection(context);
                    context.State.Memory.Set(Id, "totalIterations", collection.Count);
                    context.State.Logs.Add($"Loop de coleção inicializado com {collection.Count} itens.", Enums.LogSeverity.Information);
                    break;

                case LoopType.While:
                    context.State.Memory.Set(Id, "totalIterations", int.MaxValue); // Indefinido para while
                    context.State.Logs.Add("Loop while inicializado", Enums.LogSeverity.Information);
                    break;
            }
        }

        private bool ShouldContinueLoop(ActivityExecutionContext context, LoopType loopType)
        {
            var isLoopComplete = context.State.Memory.Get<bool>(Id, "isLoopComplete");
            if (isLoopComplete)
                return false;

            var currentIndex = context.State.Memory.Get<int>(Id, "currentIndex");
            var maxIterations = MaxIterations?.GetValue(context) ?? LoopConstants.DefaultMaxIterations;

            // Proteção contra loops infinitos
            if (currentIndex >= maxIterations)
            {
                context.State.Logs.Add($"Loop interrompido: atingiu o máximo de {maxIterations} iterações", Enums.LogSeverity.Warning);
                return false;
            }

            switch (loopType)
            {
                case LoopType.Count:
                    var endCount = EndCount?.GetValue(context) ?? 0;
                    return currentIndex <= endCount;

                case LoopType.Collection:
                    var collection = GetCollection(context);
                    return currentIndex < collection.Count;

                case LoopType.While:
                    var condition = WhileCondition?.GetValue(context);
                    if (string.IsNullOrEmpty(condition))
                    {
                        context.State.Logs.Add("Condição while vazia, loop não continuará", Enums.LogSeverity.Warning);
                        return false;
                    }

                    try
                    {
                        var expression = new Expression(condition);
                        if (expression.HasErrors())
                        {
                            context.State.Logs.Add($"Erro na condição while: {expression.Error.Message}", Enums.LogSeverity.Error);
                            return false;
                        }

                        var result = expression.Evaluate();
                        if (result is bool boolResult)
                        {
                            return boolResult;
                        }
                        else
                        {
                            context.State.Logs.Add($"A condição while '{condition}' não retornou um valor booleano", Enums.LogSeverity.Error);
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        context.State.Logs.Add($"Erro ao avaliar condição while: {ex.Message}", Enums.LogSeverity.Error);
                        return false;
                    }

                default:
                    return false;
            }
        }

        private void SetLoopVariables(ActivityExecutionContext context, LoopType loopType)
        {
            var indexVarName = IndexVariableName?.GetValue(context) ?? LoopConstants.DefaultIndexVariableName;
            var currentIndex = context.State.Memory.Get<int>(Id, "currentIndex");

            context.State.Variables.Set(indexVarName, currentIndex);

            var currentItemVarName = CurrentItemVariableName?.GetValue(context) ?? LoopConstants.DefaultCurrentItemVariableName;

            switch (loopType)
            {
                case LoopType.Count:
                    context.State.Variables.Set(currentItemVarName, currentIndex);
                    break;

                case LoopType.Collection:
                    var collection = GetCollection(context);
                    if (collection.Count > 0)
                    {
                        var currentItem = collection.ElementAt(currentIndex);
                        context.State.Variables.Set(currentItemVarName, currentItem);
                    }
                    break;

                case LoopType.While:
                    context.State.Variables.Set(currentItemVarName, currentIndex);
                    break;
            }
        }

        private void IncrementLoop(ActivityExecutionContext context)
        {
            var currentIndex = context.State.Memory.Get<int>(Id, "currentIndex");
            currentIndex++;

            context.State.Memory.Set(Id, "currentIndex", currentIndex);
        }

        private void FinalizeLoop(ActivityExecutionContext context)
        {
            context.State.Memory.Set(Id, "isLoopComplete", true);
            context.State.Memory.Set(Id, "phase", LoopConstants.PhaseCompleted);

            var currentIndex = context.State.Memory.Get<int>(Id, "currentIndex");

            var loopType = Type?.GetValue(context) ?? LoopType.Count;

            context.State.Logs.Add($"Loop {loopType} concluído após {currentIndex} iterações", Enums.LogSeverity.Information);
        }

        private ICollection<object> GetCollection(ActivityExecutionContext context)
        {
            var collectionName = Collection?.GetValue(context);

            if (!string.IsNullOrEmpty(collectionName))
            {
                var variableName = collectionName.Trim();
                if (context.State.Variables.TryGet(variableName, out var variable) && variable is not null)
                {
                    if (variable.GetValue() is ICollection<object> list)
                    {
                        return list;
                    }
                    else if (variable.GetValue() is IEnumerable<object> enumerable)
                    {
                        return enumerable.ToList();
                    }
                    else
                    {
                        throw new InvalidOperationException($"A variável '{variableName}' não é uma coleção válida.");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"A variável '{variableName}' não foi encontrada.");
                }
            }
            else
            {
                return new List<object>();
            }
        }
    }
}