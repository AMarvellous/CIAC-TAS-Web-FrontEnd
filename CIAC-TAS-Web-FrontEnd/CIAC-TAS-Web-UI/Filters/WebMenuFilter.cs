using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Refit;
using System.Text.Json;

namespace CIAC_TAS_Web_UI.Filters
{
    public class WebMenuFilter : IAsyncPageFilter
    {
        private readonly IConfiguration _configuration;
        private readonly EstudianteSession _estudianteSession;
        public WebMenuFilter(IConfiguration configuration, EstudianteSession estudianteSession)
        {
            _configuration = configuration;
            _estudianteSession = estudianteSession;
        }

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            //Check the menus here
            if (!context.HttpContext.Request.Path.Equals("/") && !context.HttpContext.Request.Path.Equals("/Login") &&
                !string.IsNullOrEmpty(context.HttpContext.Session.GetString(Session.SessionUserName)) &&
                !string.IsNullOrEmpty(context.HttpContext.Session.GetString(Session.SessionToken)) &&
                !string.IsNullOrEmpty(context.HttpContext.Session.GetString(Session.SessionRoles)) &&
                !string.IsNullOrEmpty(context.HttpContext.Session.GetString(Session.SessionUserId)) &&
                string.IsNullOrEmpty(context.HttpContext.Session.GetString(Session.SessionMenus)))
            {
                var roleName = context.HttpContext.Session.GetString(Session.SessionRoles);
                var menuModulosWebApi = RestService.For<IMenuModulosWebServiceApi>(_configuration.GetValue<string>("ServiceUrl"), 
                    new RefitSettings
                    {
                        AuthorizationHeaderValueGetter = () => Task.FromResult(context.HttpContext.Session.GetString(Session.SessionToken))
                    });
                var menuModulosResponse = await menuModulosWebApi.GetByRoleAsync(roleName);

                //Inhabilitacion Estudiante
                var inhabilitacionEstudianteServiceApi = RestService.For<IInhabilitacionEstudianteServiceApi>(_configuration.GetValue<string>("ServiceUrl"),
                    new RefitSettings
                    {
                        AuthorizationHeaderValueGetter = () => Task.FromResult(context.HttpContext.Session.GetString(Session.SessionToken))
                    });

                
                var usuarioBloqueado = false;
                var motivoBloqueo = string.Empty;
                if (roleName.Contains(RolesHelper.ESTUDIANTE_ROLE))
                {
                    var userId = context.HttpContext.Session.GetString(Session.SessionUserId);
                    var sessionToken = context.HttpContext.Session.GetString(Session.SessionToken);
                    var estudianteId = await _estudianteSession.GetEstudianteIdByUserIdAsync(userId, sessionToken);
                    var inhabilitacionEstudianteResponse = await inhabilitacionEstudianteServiceApi.GetByEstudianteIdAsync(estudianteId);

                    if (inhabilitacionEstudianteResponse.IsSuccessStatusCode)
                    {
                        usuarioBloqueado = true;
                        motivoBloqueo = inhabilitacionEstudianteResponse.Content.Motivo;
                    }
                }

                if (menuModulosResponse.IsSuccessStatusCode)
                {
                    if (!usuarioBloqueado)
                    {
                        var serializedData = JsonSerializer.Serialize(menuModulosResponse.Content.Data);
                        context.HttpContext.Session.SetString(Session.SessionMenus, serializedData);
                    } else
                    {
                        IEnumerable<MenuModulosWebResponse> menuModulosWebEmpty = new List<MenuModulosWebResponse>();
                        var serializedData = JsonSerializer.Serialize(menuModulosWebEmpty);
                        context.HttpContext.Session.SetString(Session.SessionMenus, serializedData);
                        context.HttpContext.Session.SetString(Session.SessionMotivoBloqueo, motivoBloqueo);
                    }
                    
                }
            }

            await next();
        }

        public async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            //Check the menus here      
        }
    }
}
