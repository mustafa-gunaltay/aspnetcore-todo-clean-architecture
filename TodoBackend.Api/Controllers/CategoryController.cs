using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TodoBackend.Application.Features.TodoCategory.Commands.CreateCategory;
using TodoBackend.Application.Features.TodoCategory.Commands.UpdateCategory;
using TodoBackend.Application.Features.TodoCategory.Commands.DeleteCategory;
using TodoBackend.Application.Features.TodoCategory.Queries.GetAllCategories;
using TodoBackend.Application.Features.TodoCategory.Queries.GetCategoryById;
using TodoBackend.Application.Features.BuildingBlocks;

namespace TodoBackend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public CategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    //[SwaggerOperation("Create Category")]
    //[SwaggerResponse(StatusCodes.Status201Created, "Created", typeof(Result<int>))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result<int>))]
    //[SwaggerResponse(StatusCodes.Status409Conflict, "Category name already exists", typeof(Result<int>))]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        if (result.IsSuccess)
        {
            // 201 Created - Yeni kaynak oluşturuldu, Location header ile kaynak URI'sini ver
            return Created($"/api/category/{result.Value}", result);
        }
        
        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);
            
        // Business rule violations (örn: duplicate name) için 409 Conflict
        return Conflict(result);
    }

    [HttpGet]
    //[SwaggerOperation("Get All Categories")]
    //[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(Result<IReadOnlyList<TodoBackend.Application.ViewModels.CategoryViewModel>>))]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllCategoriesQuery(), cancellationToken);
        if (result.IsSuccess)
        {
            // 200 OK - Sorgu başarılı ve data döndürüyoruz
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("{id}")]
    //[SwaggerOperation("Get Category By Id")]
    //[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(Result<TodoBackend.Application.ViewModels.CategoryViewModel>))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid CategoryId", typeof(Result<TodoBackend.Application.ViewModels.CategoryViewModel>))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "Category not found", typeof(Result<TodoBackend.Application.ViewModels.CategoryViewModel>))]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(id), cancellationToken);
        if (result.IsSuccess)
        {
            // 200 OK - Tek kaynak getirme başarılı
            return Ok(result);
        }
        
        // Validation errors (örn: id <= 0) için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);
            
        // Kaynak bulunamadı için 404 Not Found
        return NotFound(result);
    }

    [HttpPut("{id}")]
    //[SwaggerOperation("Update Category")]
    //[SwaggerResponse(StatusCodes.Status204NoContent, "Updated successfully")]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "Category not found", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status409Conflict, "Category name already exists", typeof(Result))]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Route'dan gelen id ile request'teki id'yi senkronize et
        var command = request with { CategoryId = id };
        
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsSuccess)
        {
            // 204 No Content - Güncelleme başarılı ama dönecek gövde yok
            return NoContent();
        }
        
        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);
            
        // Category not found için 404 Not Found
        if (result.Errors.Any(e => e.Contains("not found")))
            return NotFound(result);
            
        // Business rule violations (örn: duplicate name) için 409 Conflict
        return Conflict(result);
    }

    [HttpDelete("{id}")]
    //[SwaggerOperation("Delete Category")]
    //[SwaggerResponse(StatusCodes.Status204NoContent, "Deleted successfully")]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "Category not found", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status409Conflict, "Cannot delete category with active tasks", typeof(Result))]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
        if (result.IsSuccess)
        {
            // 204 No Content - Silme başarılı (soft delete), dönecek gövde yok
            return NoContent();
        }
        
        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);
            
        // Category not found için 404 Not Found
        if (result.Errors.Any(e => e.Contains("not found")))
            return NotFound(result);
            
        // Business rule violations (örn: active tasks var) için 409 Conflict
        return Conflict(result);
    }
}
