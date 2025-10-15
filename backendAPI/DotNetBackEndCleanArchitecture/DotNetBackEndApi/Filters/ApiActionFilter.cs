using Microsoft.AspNetCore.Mvc.Filters;

namespace DotNetBackEndApi.Filters
{
    public class ApiActionFilter : IActionFilter
    {
        private readonly IWebHostEnvironment _env;
        public ApiActionFilter(IWebHostEnvironment env)
        {
            _env = env;
        }

        void IActionFilter.OnActionExecuted(ActionExecutedContext context)
        {
            //string rootRoot = $@"{_env.ContentRootPath}\wwwroot\UploadFiles\aaa\";
            //throw new NotImplementedException();
        }

        void IActionFilter.OnActionExecuting(ActionExecutingContext context)
        {

            //string rootRoot = $@"{_env.ContentRootPath}\wwwroot\UploadFiles\aaa\";
        }
    }
}
