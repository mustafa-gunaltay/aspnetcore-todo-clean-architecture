using MediatR;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Domain.DomainExceptions;
using System;

namespace TodoBackend.Application.Features.TodoCategory.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, int>
{
    private readonly ITodoCleanArchitectureUnitOfWork _uow;

    public UpdateCategoryCommandHandler(ITodoCleanArchitectureUnitOfWork uow)
        => _uow = uow;

    public async Task<int> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        // 1) Varlığı kontrol et (soft-deleted ise güncellemeye izin verme)
        var category = await _uow.CategoryRepository.GetByIdAsync(request.CategoryId, ct);
        if (category is null || category.IsDeleted)
            throw new ApplicationException("Category not found.");

        // 2) İsim değişiyorsa tekillik kontrolü (kullanıcıya özel tekillik repo tarafında sağlanmalı)
        if (!string.Equals(category.Name, request.Name, StringComparison.Ordinal))
        {
            var exists = await _uow.CategoryRepository.NameExistsAsync(request.Name, ct);
            if (exists)
                throw new ApplicationException("Category name must be unique.");
            category.Rename(request.Name);                 // Domain kuralı: boş/whitespace olamaz
        }

        // 3) Açıklamayı güncelle
        category.SetDescription(request.Description);

        
        // 4) Kaydet
        await _uow.CategoryRepository.UpdateAsync(category, ct);
        var saved = await _uow.SaveChangesAsync(ct);
        if (!saved)
            throw new DomainException("Changes could not be saved.");

        return category.Id;
    }
}
