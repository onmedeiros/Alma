using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Core.ApprovalsAndChecks.Models;
using Alma.Workflows.Core.Contexts;
using Alma.Workflows.Customizations;
using Alma.Workflows.Models.Activities;
using NCalc;
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

        #region Data

        /// <summary>
        /// Índice atual da iteração do loop
        /// </summary>
        public Data<int>? CurrentIndex { get; set; }

        /// <summary>
        /// Número total de iterações esperadas
        /// </summary>
        public Data<int>? TotalIterations { get; set; }

        /// <summary>
        /// Indica se o loop foi inicializado
        /// </summary>
        public Data<bool>? IsLoopInitialized { get; set; }

        /// <summary>
        /// Indica se o loop foi completado
        /// </summary>
        public Data<bool>? IsLoopComplete { get; set; }

        /// <summary>
        /// Armazena a coleção para iteração (tipo Collection)
        /// </summary>
        public Data<List<object>>? CollectionData { get; set; }

        /// <summary>
        /// Indica qual fase do loop estamos (Start, WaitingBody, BodyCompleted, Completed)
        /// </summary>
        public Data<string>? LoopPhase { get; set; }

        #endregion

        public override IsReadyResult IsReadyToExecute(ActivityExecutionContext context)
        {
            // Se o loop já foi completado, não executar novamente
            if (IsLoopComplete?.Value == true)
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
                var phase = LoopPhase?.Value ?? LoopConstants.PhaseStart;
                var isLoopInitialized = IsLoopInitialized?.Value ?? false;

                context.State.Log($"Loop executando na fase: {phase}, Iteração: {CurrentIndex?.Value ?? 0}", Enums.LogSeverity.Debug);

                // Phase 1: Initialization
                if (phase == LoopConstants.PhaseStart || !isLoopInitialized)
                {
                    // Initilize loop if it's not initialized
                    if (!isLoopInitialized)
                    {
                        InitializeLoop(context, loopType);
                        IsLoopInitialized = new Data<bool> { Value = true };
                    }

                    // Verify if should continue
                    if (ShouldContinueLoop(context, loopType))
                    {
                        SetLoopVariables(context, loopType);

                        // Change to waiting body phase
                        LoopPhase = new Data<string> { Value = LoopConstants.PhaseWaitingBody };

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
                    IncrementLoop();

                    // Reseta a fase para início
                    LoopPhase = new Core.Activities.Base.Data<string> { Value = LoopConstants.PhaseStart };

                    // Verifica se deve continuar
                    if (ShouldContinueLoop(context, loopType))
                    {
                        SetLoopVariables(context, loopType);

                        // Muda para a fase de aguardar corpo completo
                        LoopPhase = new Core.Activities.Base.Data<string> { Value = LoopConstants.PhaseWaitingBody };

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
                context.State.Log($"Erro no loop: {ex.Message}", Enums.LogSeverity.Error);
                FinalizeLoop(context);
                Error.Execute(ex.Message);
            }
        }

        private void InitializeLoop(ActivityExecutionContext context, LoopType loopType)
        {
            CurrentIndex = new Data<int> { Value = 0 };
            IsLoopComplete = new Data<bool> { Value = false };
            LoopPhase = new Data<string> { Value = LoopConstants.PhaseStart };

            switch (loopType)
            {
                case LoopType.Count:
                    var start = StartCount?.GetValue(context) ?? 0;
                    var end = EndCount?.GetValue(context) ?? 0;
                    TotalIterations = new Core.Activities.Base.Data<int> { Value = Math.Max(0, end - start + 1) };
                    CurrentIndex.Value = start;
                    context.State.Log($"Loop de contagem inicializado: {start} até {end}", Enums.LogSeverity.Information);
                    break;

                case LoopType.Collection:
                    var collectionValue = Collection?.GetValue(context);
                    if (!string.IsNullOrEmpty(collectionValue))
                    {
                        // Tenta deserializar como coleção JSON
                        try
                        {
                            var collection = System.Text.Json.JsonSerializer.Deserialize<List<object>>(collectionValue);
                            CollectionData = new Core.Activities.Base.Data<List<object>> { Value = collection };
                            TotalIterations = new Core.Activities.Base.Data<int> { Value = collection?.Count ?? 0 };
                            context.State.Log($"Loop de coleção inicializado com {collection?.Count ?? 0} itens (JSON)", Enums.LogSeverity.Information);
                        }
                        catch
                        {
                            // Se falhar, trata como nome de variável
                            var variableName = collectionValue.Trim();
                            if (context.State.Variables.TryGetValue(variableName, out var variable))
                            {
                                if (variable.Value is IList list)
                                {
                                    var collection = new List<object>();
                                    foreach (var item in list)
                                    {
                                        collection.Add(item);
                                    }
                                    CollectionData = new Core.Activities.Base.Data<List<object>> { Value = collection };
                                    TotalIterations = new Core.Activities.Base.Data<int> { Value = collection.Count };
                                    context.State.Log($"Loop de coleção inicializado com {collection.Count} itens (variável '{variableName}')", Enums.LogSeverity.Information);
                                }
                                else if (variable.Value is IEnumerable<object> enumerable)
                                {
                                    var collection = enumerable.ToList();
                                    CollectionData = new Core.Activities.Base.Data<List<object>> { Value = collection };
                                    TotalIterations = new Core.Activities.Base.Data<int> { Value = collection.Count };
                                    context.State.Log($"Loop de coleção inicializado com {collection.Count} itens (variável '{variableName}')", Enums.LogSeverity.Information);
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
                    }
                    else
                    {
                        CollectionData = new Core.Activities.Base.Data<List<object>> { Value = new List<object>() };
                        TotalIterations = new Core.Activities.Base.Data<int> { Value = 0 };
                        context.State.Log("Loop de coleção inicializado vazio", Enums.LogSeverity.Warning);
                    }
                    break;

                case LoopType.While:
                    TotalIterations = new Core.Activities.Base.Data<int> { Value = int.MaxValue }; // Indefinido para while
                    context.State.Log("Loop while inicializado", Enums.LogSeverity.Information);
                    break;
            }
        }

        private bool ShouldContinueLoop(ActivityExecutionContext context, LoopType loopType)
        {
            if (IsLoopComplete?.Value == true)
                return false;

            var currentIndex = CurrentIndex?.Value ?? 0;
            var maxIterations = MaxIterations?.GetValue(context) ?? LoopConstants.DefaultMaxIterations;

            // Proteção contra loops infinitos
            if (currentIndex >= maxIterations)
            {
                context.State.Log($"Loop interrompido: atingiu o máximo de {maxIterations} iterações", Enums.LogSeverity.Warning);
                return false;
            }

            switch (loopType)
            {
                case LoopType.Count:
                    var endCount = EndCount?.GetValue(context) ?? 0;
                    return currentIndex <= endCount;

                case LoopType.Collection:
                    return currentIndex < (CollectionData?.Value?.Count ?? 0);

                case LoopType.While:
                    var condition = WhileCondition?.GetValue(context);
                    if (string.IsNullOrEmpty(condition))
                    {
                        context.State.Log("Condição while vazia, loop não continuará", Enums.LogSeverity.Warning);
                        return false;
                    }

                    try
                    {
                        var expression = new Expression(condition);
                        if (expression.HasErrors())
                        {
                            context.State.Log($"Erro na condição while: {expression.Error.Message}", Enums.LogSeverity.Error);
                            return false;
                        }

                        var result = expression.Evaluate();
                        if (result is bool boolResult)
                        {
                            return boolResult;
                        }
                        else
                        {
                            context.State.Log($"A condição while '{condition}' não retornou um valor booleano", Enums.LogSeverity.Error);
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        context.State.Log($"Erro ao avaliar condição while: {ex.Message}", Enums.LogSeverity.Error);
                        return false;
                    }

                default:
                    return false;
            }
        }

        private void SetLoopVariables(ActivityExecutionContext context, LoopType loopType)
        {
            var indexVarName = IndexVariableName?.GetValue(context) ?? LoopConstants.DefaultIndexVariableName;
            var currentIndex = CurrentIndex?.Value ?? 0;
            context.State.SetVariable(indexVarName, currentIndex);

            var currentItemVarName = CurrentItemVariableName?.GetValue(context) ?? LoopConstants.DefaultCurrentItemVariableName;

            switch (loopType)
            {
                case LoopType.Count:
                    context.State.SetVariable(currentItemVarName, currentIndex);
                    break;

                case LoopType.Collection:
                    if (CollectionData?.Value != null && currentIndex < CollectionData.Value.Count)
                    {
                        var currentItem = CollectionData.Value[currentIndex];
                        context.State.SetVariable(currentItemVarName, currentItem);
                    }
                    break;

                case LoopType.While:
                    context.State.SetVariable(currentItemVarName, currentIndex);
                    break;
            }
        }

        private void IncrementLoop()
        {
            if (CurrentIndex?.Value != null)
            {
                CurrentIndex.Value = CurrentIndex.Value + 1;
            }
        }

        private void FinalizeLoop(ActivityExecutionContext context)
        {
            IsLoopComplete = new Core.Activities.Base.Data<bool> { Value = true };
            LoopPhase = new Core.Activities.Base.Data<string> { Value = LoopConstants.PhaseCompleted };

            var iterations = CurrentIndex?.Value ?? 0;
            var loopType = Type?.GetValue(context) ?? LoopType.Count;

            context.State.Log($"Loop {loopType} concluído após {iterations} iterações", Enums.LogSeverity.Information);
        }
    }
}