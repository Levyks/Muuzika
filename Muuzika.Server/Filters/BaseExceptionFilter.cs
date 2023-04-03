using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Muuzika.Server.Exceptions;
using Muuzika.Server.Mappers.Interfaces;

namespace Muuzika.Server.Filters;

public class BaseExceptionFilter: IActionFilter, IOrderedFilter
{
    public int Order => int.MaxValue - 10;
    
    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is not BaseException baseException) return;
        
        var exceptionMapper = context.HttpContext.RequestServices.GetRequiredService<IExceptionMapper>();
        context.Result = new JsonResult(exceptionMapper.ToDto(baseException))
        {
            StatusCode = (int) baseException.StatusCode
        };
        context.ExceptionHandled = true;
    }
}