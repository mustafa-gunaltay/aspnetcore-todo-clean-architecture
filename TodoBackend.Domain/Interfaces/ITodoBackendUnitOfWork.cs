using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoBackend.Domain.Interfaces;

public interface ITodoBackendUnitOfWork : BuildingBlocks.IUnitOfWork
{
    
    public ICategoryRepository CategoryRepository { get; init; }
    public ITaskItemRepository TaskItemRepository { get; init; }
    public IUserRepository UserRepository { get; init; }
    public ITaskItemCategoryRepository TaskItemCategoryRepository { get; init; }

}
