using DomainEntityDTO.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DotNetBackEndApi.Filters
{
    public class ApiResourceFilter : IResultFilter
    {
        void IResultFilter.OnResultExecuted(ResultExecutedContext context)
        {

        }

        void IResultFilter.OnResultExecuting(ResultExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {

                context.Result = new JsonResult(new ApiResultModel<bool>()
                {
                    Result = false,
                    returnStatus = 400,
                    returnMsg = string.Join(",", context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            }
        }
    }
}
