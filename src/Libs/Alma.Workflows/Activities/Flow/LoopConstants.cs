namespace Alma.Workflows.Activities.Flow
{
    /// <summary>
    /// Constantes utilizadas pela atividade de Loop e coordenação com o FlowRunner.
    /// </summary>
    public static class LoopConstants
    {
        #region Loop Phases

        /// <summary>
        /// Fase inicial do loop - inicialização ou início de nova iteração
        /// </summary>
        public const string PhaseStart = "Start";

        /// <summary>
        /// Fase de aguardo - corpo do loop está sendo executado
        /// </summary>
        public const string PhaseWaitingBody = "WaitingBody";

        /// <summary>
        /// Fase de completamento - corpo foi completado, pronto para incrementar
        /// </summary>
        public const string PhaseBodyCompleted = "BodyCompleted";

        /// <summary>
        /// Fase final - loop foi concluído
        /// </summary>
        public const string PhaseCompleted = "Completed";

        #endregion

        #region Default Variable Names

        /// <summary>
        /// Nome padrão da variável que armazena o índice atual do loop
        /// </summary>
        public const string DefaultIndexVariableName = "loopIndex";

        /// <summary>
        /// Nome padrão da variável que armazena o item atual do loop
        /// </summary>
        public const string DefaultCurrentItemVariableName = "currentItem";

        #endregion

        #region Default Values

        /// <summary>
        /// Valor padrão para o máximo de iterações (proteção contra loops infinitos)
        /// </summary>
        public const int DefaultMaxIterations = 10000;

        #endregion
    }
}
