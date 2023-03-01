using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using Refit;
using System.Configuration;

namespace CIAC_TAS_Web_UI.Helper
{
    public class CuestionarioASAHelper : ICuestionarioASAHelper
    {
        //public const int NUMERO_PREGUNTAS_DEFAULT = 100;
        //public const int NUMERO_PREGUNTA_MAXIMA = 2571;
        //public const int PREGUNTA_INI_DEFAULT = 0;
        //public const int PREGUNTA_FIN_DEFAULT = 0;
        //public const int TIEMPO_LIMITE_DEFAULT = 60; //Min
        //public const double TIEMPO_POR_PREGUNTA_DEFAULT = 1; //Min

        private readonly IConfiguration _configuration;

        public CuestionarioASAHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Tuple<bool, string>> UserHasExamenProgramadoAsync(string userId, string sessionToken)
        {
            //Get Configuracion and check if we're at time of exam
            var configuracionPreguntaAsaServiceApi = GetIConfiguracionPreguntaAsaServiceApi(sessionToken);
            var configuracionPreguntaAsaServiceResponse = await configuracionPreguntaAsaServiceApi.GetAllAsync();

            if (!configuracionPreguntaAsaServiceResponse.IsSuccessStatusCode)
            {
                return new Tuple<bool, string>(false, "Ocurrio un error inesperado, vuela a intentar cargar la pagina");
            }

            var estudianteServiceApi = GetIEstudianteServiceApi(sessionToken);
            var estudianteResponse = await estudianteServiceApi.GetByUserIdAsync(userId);
            var estudiante = estudianteResponse.Content;

            if (estudiante == null)
            {
                return new Tuple<bool, string>(false, "No se encuentra un examen para este usuario");
            }

            var estudianteGrupoServiceApi = GetIEstudianteGrupoServiceApi(sessionToken);
            var estudianteGrupoResponse = await estudianteGrupoServiceApi.GetAllAsync();
            var estudianteGrupos = estudianteGrupoResponse.Content?.Data;

            if (estudianteGrupos == null)
            {
                return new Tuple<bool, string>(false, "No se encuentra un examen para este estudiante");
            }

            var datetimeNow = DateTime.Now;
            var configuracionPreguntaAsaResponses = configuracionPreguntaAsaServiceResponse.Content.Data;
            var estudianteGruposFiltered = estudianteGrupos.Where(x => x.EstudianteId == estudiante.Id).ToList();

            var existeConfiguracionExamen = false;

            foreach (var item in estudianteGruposFiltered)
            {
                var configuracionPreguntaFiltered = configuracionPreguntaAsaResponses
                    .Where(x => x.GrupoId == item.GrupoId && datetimeNow >= x.FechaInicial && datetimeNow <= x.FechaFin)
                    .FirstOrDefault();
                if (configuracionPreguntaFiltered != null)
                {
                    existeConfiguracionExamen = true;
                    break;
                }
            }

            return new Tuple<bool, string>(existeConfiguracionExamen, string.Empty);
        }

        public async Task<ConfiguracionPreguntaAsaResponse?> ConfiguracionPreguntaAsaExamNowAsync(string userId, string sessionToken)
        {
            //Get Configuracion and check if we're at time of exam
            var configuracionPreguntaAsaServiceApi = GetIConfiguracionPreguntaAsaServiceApi(sessionToken);
            var configuracionPreguntaAsaServiceResponse = await configuracionPreguntaAsaServiceApi.GetAllAsync();

            if (!configuracionPreguntaAsaServiceResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var estudianteServiceApi = GetIEstudianteServiceApi(sessionToken);
            var estudianteResponse = await estudianteServiceApi.GetByUserIdAsync(userId);
            var estudiante = estudianteResponse.Content;

            if (estudiante == null)
            {
                return null;
            }

            var estudianteGrupoServiceApi = GetIEstudianteGrupoServiceApi(sessionToken);
            var estudianteGrupoResponse = await estudianteGrupoServiceApi.GetAllAsync();
            var estudianteGrupos = estudianteGrupoResponse.Content?.Data;

            if (estudianteGrupos == null)
            {
                return null;
            }

            var datetimeNow = DateTime.Now;
            var configuracionPreguntaAsaResponses = configuracionPreguntaAsaServiceResponse.Content.Data;
            var estudianteGruposFiltered = estudianteGrupos.Where(x => x.EstudianteId == estudiante.Id).ToList();

            var existeConfiguracionExamen = false;
            //ConfiguracionPreguntaAsaResponse configuracionPreguntaFiltered;

            foreach (var item in estudianteGruposFiltered)
            {
                var configuracionPreguntaFiltered = configuracionPreguntaAsaResponses
                    .Where(x => x.GrupoId == item.GrupoId && datetimeNow >= x.FechaInicial && datetimeNow <= x.FechaFin)
                    .FirstOrDefault();
                if (configuracionPreguntaFiltered != null)
                {
                    return configuracionPreguntaFiltered;
                }
            }

            return null;
        }

        private IEstudianteGrupoServiceApi GetIEstudianteGrupoServiceApi(string sessionToken)
        {
            return RestService.For<IEstudianteGrupoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(sessionToken)
            });
        }

        private IEstudianteServiceApi GetIEstudianteServiceApi(string sessionToken)
        {
            return RestService.For<IEstudianteServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(sessionToken)
            });
        }

        private IConfiguracionPreguntaAsaServiceApi GetIConfiguracionPreguntaAsaServiceApi(string sessionToken)
        {
            return RestService.For<IConfiguracionPreguntaAsaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(sessionToken)
            });
        }
    }
}
