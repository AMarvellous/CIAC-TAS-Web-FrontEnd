using CIAC_TAS_Web_UI.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CIAC_TAS_Web_UI.Filters
{
    public class LoginFilter : IAsyncPageFilter
    {
        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            if (context.HttpContext.Request.Path.Equals("/") || context.HttpContext.Request.Path.Equals("/Login"))
            {
                // Check the Session
                if (!string.IsNullOrEmpty(context.HttpContext.Session.GetString(Session.SessionUserName)) &&
                    !string.IsNullOrEmpty(context.HttpContext.Session.GetString(Session.SessionToken)))
                {
                    context.Result = new RedirectResult("/Index");
                    await context.Result.ExecuteResultAsync(context);
                    return;
                }
            } else if (string.IsNullOrEmpty(context.HttpContext.Session.GetString(Session.SessionUserName)) ||
                    string.IsNullOrEmpty(context.HttpContext.Session.GetString(Session.SessionToken))) 
            {
                context.Result = new RedirectResult("/Login");
                await context.Result.ExecuteResultAsync(context);
                return;
            }

            await next();
        }

        public async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {

        }
    }
}
