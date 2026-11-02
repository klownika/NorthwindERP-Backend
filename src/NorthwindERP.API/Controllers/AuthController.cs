using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthwindERP.API.DTOs;
using NorthwindERP.API.Extensions;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;

namespace NorthwindERP.API.Controllers;

/// <summary>🔐 Módulo 6 — Autenticación y Seguridad</summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Valida usuario/contraseña contra useraccounts y devuelve un token JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(request.Username, request.Password, ct);
        return Ok(result.ToDto());
    }


    /// <summary>Devuelve los datos del usuario dueño del token (útil para el frontend).</summary>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<CurrentUserDto> Me()
        => Ok(new CurrentUserDto(
            User.GetUserId(),
            User.FindFirst(CustomClaims.Username)?.Value,
            User.FindFirst(CustomClaims.Role)?.Value,
            User.GetEmployeeIdOrNull()));
}
