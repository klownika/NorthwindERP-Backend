namespace NorthwindERP.Core.Entities;

/// <summary>Tabla: roles</summary>
public class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}

/// <summary>Tabla: useraccounts</summary>
public class UserAccount
{
    public int UserAccountId { get; set; }
    public int? EmployeeId { get; set; }
    public string Username { get; set; } = string.Empty;

    /// <summary>Hash BCrypt ($2a$/$2b$...). Nunca guardar la contraseña en texto plano.</summary>
    public string PasswordHash { get; set; } = string.Empty;
    public int? RoleId { get; set; }
    public bool? IsActive { get; set; }
    public bool? MustChangePassword { get; set; }
    public DateTime? CreatedAt { get; set; }

    public Role? Role { get; set; }
    public Employee? Employee { get; set; }
}
