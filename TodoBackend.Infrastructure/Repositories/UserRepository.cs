using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Domain.Interfaces.BuildingBlocks;
using TodoBackend.Domain.Models;
using TodoBackend.Infrastructure.BuildingBlocks.Implementations;

namespace TodoBackend.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(TodoBackendDbContext dbContext, ICurrentUser currentUser) 
        : base(dbContext, currentUser)
    {
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking().Where(u => u.Email == email && !u.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(u => u.Id != excludeId.Value);
        return !await query.AnyAsync(ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await Set.AsNoTracking()
            .Include(u => u.TaskItems) // TaskCount için navigation property'yi include et
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, ct);
    }

    public async Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default)
    {
        // Email ve password eşleşen aktif user var mı kontrol et
        return await Set.AsNoTracking()
            .AnyAsync(u => u.Email == email && u.Password == password && !u.IsDeleted, ct);
    }

    public async Task<int?> GetUserIdByEmailAsync(string email, CancellationToken ct = default)
    {
        // Güvenlik için - sadece ID döndür, tam kullanıcı bilgisi değil
        var user = await Set.AsNoTracking()
            .Where(u => u.Email == email && !u.IsDeleted)
            .Select(u => new { u.Id }) // Sadece ID'yi seç
            .FirstOrDefaultAsync(ct);
        
        return user?.Id;
    }

    // Base Repository metodlarını override etmeye gerek yok çünkü:
    // - GetByIdAsync() -> ReadOnlyRepository'den geliyor
    // - GetAllAsync() -> ReadOnlyRepository'den geliyor  
    // - AddAsync() -> Repository'den geliyor
    // - UpdateAsync() -> Repository'den geliyor
    // - DeleteAsync() -> Repository'den geliyor (soft delete)
    // Bu metodlar User entity'si için yeterli
}
