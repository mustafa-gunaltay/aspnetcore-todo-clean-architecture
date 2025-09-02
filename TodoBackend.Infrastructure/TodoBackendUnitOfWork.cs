using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Infrastructure.BuildingBlocks.Implementations;

namespace TodoBackend.Infrastructure;

public class TodoBackendUnitOfWork : UnitOfWork //, ITodoBackendUnitOfWork
{

    public ICategoryRepository CategoryRepository;

    //public ITaskItemRepository TaskItemRepository;

    //public IUserRepository UserRepository;

    //public ITaskItemCategoryRepository TaskItemCategoryRepository;

    public TodoBackendUnitOfWork(TodoBackendDbContext dbcontext, ICategoryRepository categoryRepository/*, ITaskItemRepository TaskItemRepository, IUserRepository UserRepository, ITaskItemCategoryRepository TaskItemCategoryRepository*/)
    : base(dbcontext)
    {
        this.CategoryRepository = categoryRepository;
        //this.TaskItemRepository = todoItemRepository;
        //this.UserRepository = userRepository;
        //this.TaskItemCategoryRepository = taskItemCategoryRepository;
    }
}
