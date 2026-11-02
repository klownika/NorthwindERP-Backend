using System.Security.Claims;
using NorthwindERP.Core.Exceptions;
using NorthwindERP.Core.Models;

namespace NorthwindERP.API.Extensions;

/// <summary>Lee datos del usuario logueado desde su token JWT (User en el controlador).</summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Empleado que está registrando la operación (p_employee_id de las RPC).
    /// Se toma del token, NO del body: así nadie puede registrar pedidos a nombre de otro.
    /// </summary>
    public static int GetEmployeeId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(CustomClaims.EmployeeId)?.Value;

        return int.TryParse(value, out var employeeId)
            ? employeeId
            : throw new BusinessRuleException(
                "Tu usuario no tiene un empleado asociado (useraccounts.employeeid), no puede registrar operaciones.");
    }

    public static int? GetEmployeeIdOrNull(this ClaimsPrincipal user)
        => int.TryParse(user.FindFirst(CustomClaims.EmployeeId)?.Value, out var id) ? id : null;

    public static int GetUserId(this ClaimsPrincipal user)
        => int.TryParse(user.FindFirst(CustomClaims.UserId)?.Value, out var id)
            ? id
            : throw new UnauthorizedAccessException("Token inválido.");
}
