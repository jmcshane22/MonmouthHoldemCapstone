using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MH.Capstone.Domain.Tools
{
    public static class Extensions
    {
        public static byte[] ToByteArray(this IFormFile formFile)
        {
            ArgumentNullException.ThrowIfNull(formFile);

            using var memoryStream = new MemoryStream();
            formFile.CopyTo(memoryStream);
            memoryStream.Position = 0;
            return memoryStream.ToArray();
        }

        // Provider-neutral check for a database error, raw or wrapped in a DbUpdateException.
        // SQL Server is matched by error number, Postgres by SQLSTATE.
        internal static bool IsDbError(this Exception ex, SqlErrorNumber errNum)
        {
            var inner = ex is DbUpdateException { InnerException: { } i } ? i : ex;
            return inner switch
            {
                SqlException sqlEx => sqlEx.Errors.Cast<SqlError>().Any(e => e.Number == (int)errNum
                    // EF creates unique *indexes*, which SQL Server reports as 2601 rather than 2627.
                    || (errNum == SqlErrorNumber.UniqueConstraintViolation && e.Number == 2601)),
                PostgresException pgEx => pgEx.SqlState == ToPostgresSqlState(errNum),
                _ => false
            };
        }

        // Unique/PK and FK/Check share SQL Server numbers, so each pair maps to one SQLSTATE.
        private static string? ToPostgresSqlState(SqlErrorNumber errNum) => errNum switch
        {
            SqlErrorNumber.UniqueConstraintViolation => PostgresErrorCodes.UniqueViolation,
            SqlErrorNumber.ForeignKeyConstraintViolation => PostgresErrorCodes.ForeignKeyViolation,
            SqlErrorNumber.DeadlockVictim => PostgresErrorCodes.DeadlockDetected,
            _ => null
        };
    }

    public static class HttpHelperMethods
    {
        public static string CreateQueryParamsFragment(params IEnumerable<KeyValuePair<string, string>>? queryParams)
        {
            var queryList = queryParams?.ToList() ?? new List<KeyValuePair<string, string>>();
            return queryList.Any() ? 
                string.Join("&", queryList.Select(kvp =>
                    $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"))
                : string.Empty;
        }
    }
}
