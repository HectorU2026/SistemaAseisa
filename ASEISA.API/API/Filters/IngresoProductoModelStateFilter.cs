using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace API.Filters
{
    public class IngresoProductoModelStateFilter : Attribute, IActionFilter, IOrderedFilter
    {
        public int Order => -3000;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                if (EsErrorDeConversion(context))
                {
                    context.Result = new BadRequestObjectResult("Debe escribir un número");
                    return;
                }

                context.Result = new BadRequestObjectResult("Complete todos los campos primero");
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        private static bool EsErrorDeConversion(ActionExecutingContext context)
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
                        || mensaje.Contains("converted to System.DateTime", StringComparison.OrdinalIgnoreCase)
                        || mensaje.Contains("The JSON value could not be converted", StringComparison.OrdinalIgnoreCase)
                        || EsErrorDeConversionDeQuery(mensaje))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool EsErrorDeConversionDeQuery(string mensaje)
        {
            return (mensaje.Contains("The value '", StringComparison.OrdinalIgnoreCase)
                    && mensaje.Contains("is not valid", StringComparison.OrdinalIgnoreCase))
                || (mensaje.Contains("El valor '", StringComparison.OrdinalIgnoreCase)
                    && mensaje.Contains("no es válido", StringComparison.OrdinalIgnoreCase));
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
