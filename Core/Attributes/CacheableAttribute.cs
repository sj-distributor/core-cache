using Core.Driver;
using Core.Entity;
using Core.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Core.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class Cacheable : Attribute, IAsyncActionFilter
{
    private readonly string _name;
    private string _key;
    private readonly long _expire;
    public readonly ICacheClient _cacheClient;

    public Cacheable(CacheableSettings cacheableSettings, ICacheClient cacheClient)
    {
        _name = cacheableSettings.Name;
        _key = cacheableSettings.Key;
        _expire = cacheableSettings.Expire;
        _cacheClient = cacheClient;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (string.IsNullOrEmpty(_key) || string.IsNullOrEmpty(_name))
        {
            await next();
        }
        else
        {
            var key = KeyGenerateHelper.GetKey(_name, _key, context.ActionArguments);
            
            var cacheString = _cacheClient.Get(key);
            
            if (!string.IsNullOrEmpty(cacheString))
            {
                context.Result = new ContentResult()
                {
                    Content = cacheString,
                    ContentType = "text/html; charset=utf-8"
                };
                return;
            }
            
            var executedContext = await next();
            
            #region do logic get response

            // set cache
            switch (executedContext.Result)
            {
                case ViewResult viewResult:
                {
                    var executor = (ViewResultExecutor)executedContext.HttpContext.RequestServices
                        .GetService<IActionResultExecutor<ViewResult>>()!;

                    var viewEngineResult = executor.FindView(context, viewResult);

                    var view = viewEngineResult.View;

                    using (view as IDisposable)
                    {
                        var viewOptions = context.HttpContext.RequestServices.GetService<IOptions<MvcViewOptions>>();

                        var writer = new StringWriter();

                        var viewContext = new ViewContext(
                            context,
                            view,
                            viewResult.ViewData,
                            viewResult.TempData,
                            writer,
                            viewOptions.Value.HtmlHelperOptions);

                        view.RenderAsync(viewContext).GetAwaiter().GetResult();
                    
                        _cacheClient.Set(key, writer.ToString(), _expire);
                    }
                    break;
                }
                case JsonResult jsonResult:
                    _cacheClient.Set(key, JsonConvert.SerializeObject(jsonResult.Value), _expire);
                    break;
                case ObjectResult objectResult:
                    _cacheClient.Set(key, JsonConvert.SerializeObject(objectResult.Value), _expire);
                    break;
            }
            #endregion
        }
    }
}