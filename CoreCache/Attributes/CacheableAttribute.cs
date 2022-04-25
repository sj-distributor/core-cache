using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
using CoreCache.Cache;
using CoreCache.Tools;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace CoreCache.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class Cacheable : Attribute, IAsyncActionFilter
{
    private readonly string _name;
    private string _key;
    private readonly long _dueTime;
    private readonly ICacheClient _cacheClient;


    public Cacheable(string name, string key, ICacheClient cacheClient, long dueTime = 0)
    {
        _name = name;
        _key = key;
        _dueTime = dueTime;
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
            var values = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonStream(
                    new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(context.ActionArguments))))
                .Build();

            //1. do logic generate key
            var reg = new Regex("\\{([^\\}]*)\\}");
            var matches = reg.Matches(_key);

            foreach (Match match in matches)
            {
                Console.WriteLine(match.Value);
                _key = _key.Replace(match.Value, values[match.Value.Replace(@"{", "").Replace(@"}", "")]);
            }

            var key = $"{_name}:{_key}";

            var cacheString = _cacheClient.Get(key);
            if (!string.IsNullOrEmpty(cacheString))
            {
                context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
                await context.HttpContext.Response.WriteAsync(cacheString);
                return;
            }

            var executedContext = await next();

            #region do logic get response

            //2. do logic get response
            if (executedContext.Result is ViewResult viewResult)
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

                    executedContext.Result = new ContentResult
                    {
                        Content = writer.ToString(),
                        ContentType = "text/html; charset=utf-8",
                        StatusCode = viewResult.StatusCode
                    };
                }
            }
            else if (executedContext.Result is JsonResult jsonResult)
            {
            }
            else if (executedContext.Result is ObjectResult objectResult)
            {
                _cacheClient.Set(key, JsonConvert.SerializeObject(objectResult.Value), _dueTime);
            }

            #endregion


            Console.WriteLine("1");
            //3. do cache key value
        }
    }
}