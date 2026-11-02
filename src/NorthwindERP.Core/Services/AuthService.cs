using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;

namespace NorthwindERP.Core.Services;

public class AuthService : IAuthService
{
    // Mismo mensaje para "usuario no existe" y "contraseña incorrecta":
    // así no se le revela a un atacante qué usuarios existen.
    private const string InvalidCredentials = "Usuario o contraseña incorrectos.";

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public AuthService(IUserRepository users, IPasswordHasher hasher, ITokenService tokens)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<LoginResult> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var user = await _users.GetByUsernameAsync(username.Trim(), ct)
                   ?? throw new UnauthorizedAccessException(InvalidCredentials);

        if (!_hasher.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException(InvalidCredentials);

        if (user.IsActive != true)
            throw new UnauthorizedAccessException("El usuario está desactivado.");

        var (token, expiresAtUtc) = _tokens.CreateToken(user);

        return new LoginResult(
            token,
            expiresAtUtc,
            user.UserAccountId,
            user.Username,
            user.Role?.RoleName,
            user.EmployeeId,
            user.MustChangePassword == true);
    }
}
