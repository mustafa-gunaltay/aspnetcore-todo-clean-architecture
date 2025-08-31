using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoBackend.Domain.Models;

namespace TodoBackend.Domain.Interfaces;

public interface IUserRepository : BuildingBlocks.IRepository<User>
{
    Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default);

    // Güvenlik için - sadece ID döndür, tam kullanıcı bilgisi değil
    Task<int?> GetUserIdByEmailAsync(string email, CancellationToken ct = default);
}
