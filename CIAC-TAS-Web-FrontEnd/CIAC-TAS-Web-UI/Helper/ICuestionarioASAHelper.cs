using CIAC_TAS_Service.Contracts.V1.Responses;

namespace CIAC_TAS_Web_UI.Helper
{
    public interface ICuestionarioASAHelper
    {
        public const int NUMERO_PREGUNTAS_DEFAULT = 100;
        public const int NUMERO_PREGUNTA_MAXIMA = 2571;
        public const int PREGUNTA_INI_DEFAULT = 0;
        public const int PREGUNTA_FIN_DEFAULT = 0;
        public const int TIEMPO_LIMITE_DEFAULT = 60; //Min
        public const double TIEMPO_POR_PREGUNTA_DEFAULT = 1; //Min
        public const int TIEMPO_LIMITE_EXAMEN_DEFAULT = 100; //Min
        public const int NUMERO_PREGUNTAS_EXAMEN_DEFAULT = 100;

        Task<Tuple<bool, string>> UserHasExamenProgramadoAsync(string userId, string sessionToken);
        Task<ConfiguracionPreguntaAsaResponse?> ConfiguracionPreguntaAsaExamNowAsync(string userId, string sessionToken);
    }
}
