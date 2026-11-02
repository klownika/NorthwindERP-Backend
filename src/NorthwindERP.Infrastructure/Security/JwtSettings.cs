namespace NorthwindERP.Infrastructure.Security;

/// <summary>Se llena desde la sección "Jwt" de appsettings.json / user-secrets.</summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>Clave secreta para firmar tokens. Mínimo 32 caracteres. NUNCA subirla a GitHub.</summary>
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 480;
}
