using Microsoft.AspNetCore.Mvc.Filters;

namespace WCCG.PAS.Referrals.API.Middleware;

public class ModelStateFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
        {
            return;
        }

        var exception = context.ModelState.Values
            .Select(v => v.Errors.FirstOrDefault(e => e.Exception is not null)?.Exception)
            .FirstOrDefault();

        if (exception is not null)
        {
            throw exception;
        }
    }
}
