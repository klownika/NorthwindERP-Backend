using Microsoft.EntityFrameworkCore;
using NorthwindERP.Core.Entities;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Infrastructure.Data;

namespace NorthwindERP.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly NorthwindDbContext _db;

    public UserRepository(NorthwindDbContext db) => _db = db;

    public Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        var normalized = username.ToLower();
        return _db.UserAccounts
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == normalized, ct);
    }

    public Task<UserAccount?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.UserAccounts
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.UserAccountId == id, ct);
}
