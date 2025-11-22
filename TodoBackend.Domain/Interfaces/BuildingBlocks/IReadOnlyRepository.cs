using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoBackend.Domain.Models.BuildingBlocks;
using TodoBackend.Domain.SpecificationConfig;

namespace TodoBackend.Domain.Interfaces.BuildingBlocks;

public interface IReadOnlyRepository<TEntity> where TEntity : Entity
{
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(int TotalCount, IReadOnlyList<TEntity> Data)> ListAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default);
}
