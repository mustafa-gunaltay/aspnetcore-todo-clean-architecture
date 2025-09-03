using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<int?> GetUserIdByEmailAsync(string email, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }


    public Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }


}
