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
        public WebMenuFilter(IConfiguration configuration)
        {
            _configuration = configuration;
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

                if (menuModulosResponse.IsSuccessStatusCode)
                {
                    //menuModulosResponse.Content.Data;
                    var serializedData = JsonSerializer.Serialize(menuModulosResponse.Content.Data);
                    context.HttpContext.Session.SetString(Session.SessionMenus, serializedData);
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
