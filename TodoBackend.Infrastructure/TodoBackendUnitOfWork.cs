using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TodoBackend.Domain.Interfaces.Out;
using TodoBackend.Infrastructure.BuildingBlocks.Implementations;

namespace TodoBackend.Infrastructure;

public class TodoBackendUnitOfWork : UnitOfWork, ITodoBackendUnitOfWork
{
    public ICategoryRepository CategoryRepository { get; init; }
    public ITaskItemRepository TaskItemRepository { get; init; }
    public IUserRepository UserRepository { get; init; }
    public ITaskItemCategoryRepository TaskItemCategoryRepository { get; init; }

    public TodoBackendUnitOfWork(
        TodoBackendDbContext dbcontext,
        ILogger<UnitOfWork> logger,
        ICategoryRepository categoryRepository,
        ITaskItemRepository taskItemRepository,
        IUserRepository userRepository,
        ITaskItemCategoryRepository taskItemCategoryRepository)
        : base(dbcontext, logger)
    {
        CategoryRepository = categoryRepository;
        TaskItemRepository = taskItemRepository;
        UserRepository = userRepository;
        TaskItemCategoryRepository = taskItemCategoryRepository;
    }
}
