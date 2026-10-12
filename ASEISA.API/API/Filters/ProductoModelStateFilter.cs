using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace API.Filters
{
    public class ProductoModelStateFilter : Attribute, IActionFilter, IOrderedFilter
    {
        public int Order => -3000;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                if (EsErrorDeConversionNumerica(context))
                {
                    context.Result = new BadRequestObjectResult("Debe escribir un número");
                    return;
                }

                context.ModelState.Clear();
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        private static bool EsErrorDeConversionNumerica(ActionExecutingContext context)
        {
            foreach (var estado in context.ModelState.Values)
            {
                foreach (var error in estado.Errors)
                {
                    if (EsExcepcionDeConversion(error.Exception))
                    {
                        return true;
                    }

                    var mensaje = error.ErrorMessage ?? string.Empty;
                    if (mensaje.Contains("could not be converted", StringComparison.OrdinalIgnoreCase)
                        || mensaje.Contains("converted to System.Int", StringComparison.OrdinalIgnoreCase)
                        || mensaje.Contains("converted to System.Decimal", StringComparison.OrdinalIgnoreCase)
                        || mensaje.Contains("The JSON value could not be converted", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool EsExcepcionDeConversion(Exception? exception)
        {
            while (exception != null)
            {
                if (exception is JsonException
                    or FormatException
                    or InvalidCastException
                    or OverflowException
                    or InputFormatterException)
                {
                    return true;
                }

                exception = exception.InnerException;
            }

            return false;
        }
    }
}
