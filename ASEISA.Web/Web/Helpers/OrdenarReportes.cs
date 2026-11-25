using System.Reflection;

namespace Web.Helpers;

public static class OrdenarReportes
{
    public static List<T> Ordenar<T>(IEnumerable<T> lista, string? propiedad, bool ascendente = true)
    {
        if (string.IsNullOrWhiteSpace(propiedad) || lista == null || !lista.Any())
            return lista?.ToList() ?? new List<T>();

        var listaOriginal = lista.ToList();
        var propertyInfo = typeof(T).GetProperty(propiedad, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (propertyInfo == null)
        {
            var propLista = typeof(T).GetProperties().FirstOrDefault(p =>
                p.PropertyType.IsGenericType &&
                p.PropertyType.GetGenericTypeDefinition() == typeof(List<>));

            if (propLista != null)
            {
                var elementoInterno = propLista.PropertyType.GetGenericArguments()[0];
                var propInterna = elementoInterno.GetProperty(propiedad, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propInterna != null)
                {
                    foreach (var item in listaOriginal)
                    {
                        var listaInterna = propLista.GetValue(item) as System.Collections.IList;
                        if (listaInterna != null)
                        {
                            var ordenada = ascendente
                                ? listaInterna.Cast<object>().OrderBy(x => propInterna.GetValue(x)).ToList()
                                : listaInterna.Cast<object>().OrderByDescending(x => propInterna.GetValue(x)).ToList();

                            listaInterna.Clear();
                            foreach (var obj in ordenada) listaInterna.Add(obj);
                        }
                    }

                    return ascendente
                        ? listaOriginal.OrderBy(x =>
                        {
                            var l = propLista.GetValue(x) as System.Collections.IList;
                            return l != null && l.Count > 0 ? propInterna.GetValue(l[0]) : null;
                        }).ToList()
                        : listaOriginal.OrderByDescending(x =>
                        {
                            var l = propLista.GetValue(x) as System.Collections.IList;
                            return l != null && l.Count > 0 ? propInterna.GetValue(l[0]) : null;
                        }).ToList();
                }
            }
            return listaOriginal; 
        }

        return ascendente
            ? listaOriginal.OrderBy(x => propertyInfo.GetValue(x, null)).ToList()
            : listaOriginal.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();
    }
}