using CIAC_TAS_Web_UI.Helper;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CIAC_TAS_Web_UI.Filters
{
    public class ExceptionHandlerFilter : IAsyncExceptionFilter
    {
        public async Task OnExceptionAsync(ExceptionContext context)
        {
            var exception = context.Exception;
            var handler = context.RouteData.Values.Keys.Contains("handler") ? context.RouteData.Values["handler"].ToString() : "Unknown Handler";
            var page = context.RouteData.Values.Keys.Contains("page") ? context.RouteData.Values["page"].ToString() : "Unknown Page";

            Logger.WriteLog(exception.Message, handler, page);
        }
    }
}
