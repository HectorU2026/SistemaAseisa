using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters
{
    public class ProductoModelStateFilter : Attribute, IActionFilter, IOrderedFilter
    {
        public int Order => -3000;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult("Debe escribir un número");
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
