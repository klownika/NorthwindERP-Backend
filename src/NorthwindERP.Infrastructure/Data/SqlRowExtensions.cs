using NorthwindERP.Core.Models;

namespace NorthwindERP.Infrastructure.Data;

internal static class SqlRowExtensions
{
    /// <summary>
    /// Convierte las filas dinámicas que devuelve Dapper (vistas / funciones RPC)
    /// en QueryRow, que se serializa a JSON con los mismos nombres de columna de la BD.
    /// </summary>
    public static IReadOnlyList<QueryRow> ToQueryRows(this IEnumerable<dynamic> rows)
    {
        var result = new List<QueryRow>();

        foreach (IDictionary<string, object> row in rows)
        {
            var queryRow = new QueryRow();
            foreach (var column in row)
                queryRow[column.Key] = column.Value;

            result.Add(queryRow);
        }

        return result;
    }
}
