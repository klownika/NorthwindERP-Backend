using NorthwindERP.Core.Interfaces;

namespace NorthwindERP.Infrastructure.Security;

/// <summary>
/// Compatible con los hashes de pgcrypto en Supabase:
/// extensions.crypt('clave', extensions.gen_salt('bf')) genera "$2a$..." y BCrypt los verifica.
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (Exception)
        {
            // El valor guardado no es un hash BCrypt válido (ej: contraseña en texto plano).
            return false;
        }
    }
}
