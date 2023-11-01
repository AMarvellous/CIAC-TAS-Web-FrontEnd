using CIAC_TAS_Service.Sdk;
using Refit;

namespace CIAC_TAS_Web_UI.Helper
{
    public class InstructorSession
    {
        private readonly IConfiguration _configuration;

        public InstructorSession(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<int> GetInstructorIdByAsync(string sessionUserId, string sessionToken)
        {
            var instructorId = 0;
            var instructorServiceApi = GetIInstructorServiceApi(sessionToken);
            var instructorResponse = await instructorServiceApi.GetByUserIdAsync(sessionUserId);

            
            if (instructorResponse.IsSuccessStatusCode)
            {
                instructorId = instructorResponse.Content.Id;
            }

            return instructorId;
        }

        private IInstructorServiceApi GetIInstructorServiceApi(string sessionToken)
        {
            return RestService.For<IInstructorServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(sessionToken)
            });
        }
    }
}
