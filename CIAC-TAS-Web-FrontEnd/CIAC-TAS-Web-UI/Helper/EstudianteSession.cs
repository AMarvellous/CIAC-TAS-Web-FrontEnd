using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using Refit;

namespace CIAC_TAS_Web_UI.Helper
{
    public class EstudianteSession
    {
        private readonly IConfiguration _configuration;

        public EstudianteSession(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<int> GetEstudianteIdByUserIdAsync(string sessionUserId, string sessionToken)
        {
            var estudianteId = 0;
            var estudianteServiceApi = GetIEstudianteServiceApi(sessionToken);
            var estudianteResponse = await estudianteServiceApi.GetByUserIdAsync(sessionUserId);

            if (estudianteResponse.IsSuccessStatusCode)
            {
                estudianteId = estudianteResponse.Content.Id;
            }

            return estudianteId;
        }

        public async Task<EstudianteResponse> GetEstudianteByUserIdAsync(string sessionUserId, string sessionToken)
        {
            var estudianteServiceApi = GetIEstudianteServiceApi(sessionToken);
            var estudianteResponse = await estudianteServiceApi.GetByUserIdAsync(sessionUserId);

            if (estudianteResponse.IsSuccessStatusCode)
            {
                return estudianteResponse.Content;
            }

            return null;
        }

        private IEstudianteServiceApi GetIEstudianteServiceApi(string sessionToken)
        {
            return RestService.For<IEstudianteServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(sessionToken)
            });
        }
    }
}
