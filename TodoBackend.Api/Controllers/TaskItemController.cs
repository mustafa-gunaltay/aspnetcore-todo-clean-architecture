using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TodoBackend.Application.Features.TodoTaskItem.Commands.CreateTaskItem;
using TodoBackend.Application.Features.TodoTaskItem.Commands.UpdateTaskItem;
using TodoBackend.Application.Features.TodoTaskItem.Commands.DeleteTaskItem;
using TodoBackend.Application.Features.TodoTaskItem.Commands.AssignTaskItemToCategory;
using TodoBackend.Application.Features.TodoTaskItem.Commands.RemoveTaskItemFromCategory;
using TodoBackend.Application.Features.TodoTaskItem.Commands.CompleteTaskItem;
using TodoBackend.Application.Features.TodoTaskItem.Commands.ReopenTaskItem;
using TodoBackend.Application.Features.TodoTaskItem.Queries.GetFilteredTaskItems;
using TodoBackend.Application.Features.TodoTaskItem.Queries.GetOverdueTaskItems;
using TodoBackend.Application.Features.TodoTaskItem.Queries.GetTaskItemsByUserId;
using TodoBackend.Application.Features.TodoTaskItem.Queries.GetUpcomingTaskItems;
using TodoBackend.Application.Features.TodoTaskItem.Queries.GetTaskItemsByCategory;
using TodoBackend.Application.Features.TodoTaskItem.Queries.GetTaskItemById;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.Enums;

namespace TodoBackend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]  // Bu controller'a erişmek için JWT token gerekli
public class TaskItemController : ControllerBase
{
    private readonly IMediator _mediator;

    public TaskItemController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    //[SwaggerOperation("Get Task Item by Id")]
    //[SwaggerResponse(StatusCodes.Status200OK, "Task retrieved successfully", typeof(Result<TaskItemViewModel>))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result<TaskItemViewModel>))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "Task not found", typeof(Result<TaskItemViewModel>))]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetTaskItemByIdQuery(id), cancellationToken);
        if (result.IsSuccess)
        {
            // 200 OK - Data retrieval başarılı
            return Ok(result);
        }

        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);

        // Task not found için 404 Not Found
        if (result.Errors.Any(e => e.Contains("not found")))
            return NotFound(result);

        // Other errors için 400 Bad Request
        return BadRequest(result);
    }

    [HttpGet("filtered-tasks")]
    //[SwaggerOperation("Get Filtered Task Items")]
    //[SwaggerResponse(StatusCodes.Status200OK, "Tasks retrieved successfully", typeof(Result<IReadOnlyList<TaskItemViewModel>>))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result<IReadOnlyList<TaskItemViewModel>>))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "User not found", typeof(Result<IReadOnlyList<TaskItemViewModel>>))]
    public async Task<IActionResult> GetFilteredTasks(
        [FromQuery] GetFilteredTaskItemsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(query, cancellationToken);
        if (result.IsSuccess)
        {
            // 200 OK - Data retrieval başarılı
            return Ok(result);
        }

        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);

        // User not found için 404 Not Found
        if (result.Errors.Any(e => e.Contains("not found")))
            return NotFound(result);

        // Other errors için 400 Bad Request
        return BadRequest(result);
    }

    [HttpGet("overdue-tasks")]
    //[SwaggerOperation("Get Overdue Task Items")]
    //[SwaggerResponse(StatusCodes.Status200OK, "Overdue tasks retrieved successfully", typeof(Result<IReadOnlyList<TaskItemViewModel>>))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result<IReadOnlyList<TaskItemViewModel>>))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "User not found", typeof(Result<IReadOnlyList<TaskItemViewModel>>))]
    public async Task<IActionResult> GetOverdueTasks(
        [FromQuery] GetOverdueTaskItemsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(query, cancellationToken);
        if (result.IsSuccess)
        {
            // 200 OK - Data retrieval başarılı
            return Ok(result);
        }

        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);

        // User not found için 404 Not Found
        if (result.Errors.Any(e => e.Contains("not found")))
            return NotFound(result);

        // Other errors için 400 Bad Request
        return BadRequest(result);
    }

    [HttpPost]
    //[SwaggerOperation("Create Task")]
    //[SwaggerResponse(StatusCodes.Status201Created, "Created", typeof(Result<int>))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result<int>))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "User not found", typeof(Result<int>))]
    public async Task<IActionResult> Create([FromBody] CreateTaskItemCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        if (result.IsSuccess)
        {
            // 201 Created - Yeni kaynak oluşturuldu, Location header ile kaynak URI'sini ver
            return Created($"/api/taskitem/{result.Value}", result);
        }
        
        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);
            
        // Business rule violations için 400 Bad Request (örn: user not found, high priority without due date)
        return BadRequest(result);
    }

    [HttpPut("{id}")]
    //[SwaggerOperation("Update Task")]
    //[SwaggerResponse(StatusCodes.Status200OK, "Updated successfully", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "Task not found", typeof(Result))]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskItemCommand request, CancellationToken cancellationToken)
    {
        // Route'dan gelen id ile request'teki id'yi senkronize et
        var command = request with { TaskItemId = id };
        
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsSuccess)
        {
            // 200 OK - Güncelleme başarılı, success message ile birlikte
            return Ok(result);
        }
        
        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);
            
        // Task not found için 404 Not Found
        if (result.Errors.Any(e => e.Contains("not found")))
            return NotFound(result);
            
        // Business rule violations için 400 Bad Request
        return BadRequest(result);
    }

    [HttpDelete("{id}")]
    //[SwaggerOperation("Delete Task")]
    //[SwaggerResponse(StatusCodes.Status200OK, "Deleted successfully", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "Task not found", typeof(Result))]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteTaskItemCommand(id), cancellationToken);
        if (result.IsSuccess)
        {
            // 200 OK - Silme başarılı, success message ile birlikte
            return Ok(result);
        }
        
        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);
            
        // Task not found için 404 Not Found
        if (result.Errors.Any(e => e.Contains("not found")))
            return NotFound(result);
            
        // Diğer hatalar için 400 Bad Request
        return BadRequest(result);
    }

    [HttpPost("assign-category")]
    //[SwaggerOperation("Assign Task to Category")]
    //[SwaggerResponse(StatusCodes.Status200OK, "Task assigned to category successfully", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "Task or Category not found", typeof(Result))]
    public async Task<IActionResult> AssignToCategory([FromBody] AssignTaskItemToCategoryCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        if (result.IsSuccess)
        {
            // 200 OK - Assignment başarılı
            return Ok(result);
        }
        
        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);
            
        // Task or Category not found için 404 Not Found
        if (result.Errors.Any(e => e.Contains("not found")))
            return NotFound(result);
            
        // Business rule violations için 400 Bad Request
        return BadRequest(result);
    }

    [HttpDelete("remove-category")]
    //[SwaggerOperation("Remove Task from Category")]
    //[SwaggerResponse(StatusCodes.Status200OK, "Task removed from category successfully", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "Task or Category not found", typeof(Result))]
    public async Task<IActionResult> RemoveFromCategory([FromBody] RemoveTaskItemFromCategoryCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        if (result.IsSuccess)
        {
            // 200 OK - Removal başarılı
            return Ok(result);
        }
        
        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);
            
        // Task or Category not found için 404 Not Found
        if (result.Errors.Any(e => e.Contains("not found")))
            return NotFound(result);
            
        // Business rule violations için 400 Bad Request
        return BadRequest(result);
    }

    [HttpPost("complete")]
    //[SwaggerOperation("Complete Task")]
    //[SwaggerResponse(StatusCodes.Status200OK, "Task completed successfully", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "Task not found", typeof(Result))]
    public async Task<IActionResult> Complete([FromBody] CompleteTaskItemCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        if (result.IsSuccess)
        {
            // 200 OK - Complete işlemi başarılı
            return Ok(result);
        }
        
        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);
            
        // Task not found için 404 Not Found
        if (result.Errors.Any(e => e.Contains("not found")))
            return NotFound(result);
            
        // Business rule violations için 400 Bad Request (örn: already completed, past due date)
        return BadRequest(result);
    }

    [HttpPost("reopen")]
    //[SwaggerOperation("Reopen Task")]
    //[SwaggerResponse(StatusCodes.Status200OK, "Task reopened successfully", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "Task not found", typeof(Result))]
    public async Task<IActionResult> Reopen([FromBody] ReopenTaskItemCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        if (result.IsSuccess)
        {
            // 200 OK - Reopen işlemi başarılı
            return Ok(result);
        }
        
        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);
            
        // Task not found için 404 Not Found
        if (result.Errors.Any(e => e.Contains("not found")))
            return NotFound(result);
            
        // Business rule violations için 400 Bad Request (örn: not completed, cannot reopen)
        return BadRequest(result);
    }

    [HttpGet("user-tasks")]
    //[SwaggerOperation("Get Task Items by User Id")]
    //[SwaggerResponse(StatusCodes.Status200OK, "User tasks retrieved successfully", typeof(Result<IReadOnlyList<TaskItemViewModel>>))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result<IReadOnlyList<TaskItemViewModel>>))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "User not found", typeof(Result<IReadOnlyList<TaskItemViewModel>>))]
    public async Task<IActionResult> GetTaskItemsByUserId(
        [FromQuery] GetTaskItemsByUserIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(query, cancellationToken);
        if (result.IsSuccess)
        {
            // 200 OK - Data retrieval başarılı
            return Ok(result);
        }

        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);

        // User not found için 404 Not Found
        if (result.Errors.Any(e => e.Contains("not found")))
            return NotFound(result);

        // Other errors için 400 Bad Request
        return BadRequest(result);
    }

    [HttpGet("upcoming-tasks")]
    //[SwaggerOperation("Get Upcoming Task Items")]
    //[SwaggerResponse(StatusCodes.Status200OK, "Upcoming tasks retrieved successfully", typeof(Result<IReadOnlyList<TaskItemViewModel>>))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result<IReadOnlyList<TaskItemViewModel>>))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "User not found", typeof(Result<IReadOnlyList<TaskItemViewModel>>))]
    public async Task<IActionResult> GetUpcomingTasks(
        [FromQuery] GetUpcomingTaskItemsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(query, cancellationToken);
        if (result.IsSuccess)
        {
            // 200 OK - Data retrieval başarılı
            return Ok(result);
        }

        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);

        // User not found için 404 Not Found
        if (result.Errors.Any(e => e.Contains("not found")))
            return NotFound(result);

        // Other errors için 400 Bad Request
        return BadRequest(result);
    }

    [HttpGet("category/{categoryId}")]
    //[SwaggerOperation("Get Task Items by Category")]
    //[SwaggerResponse(StatusCodes.Status200OK, "Task items retrieved successfully", typeof(Result<IReadOnlyList<TaskItemViewModel>>))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result<IReadOnlyList<TaskItemViewModel>>))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "Category not found", typeof(Result<IReadOnlyList<TaskItemViewModel>>))]
    public async Task<IActionResult> GetTaskItemsByCategory(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetTaskItemsByCategoryQuery(categoryId), cancellationToken);
        if (result.IsSuccess)
        {
            // 200 OK - Data retrieval başarılı
            return Ok(result);
        }

        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);

        // Category not found için 404 Not Found
        if (result.Errors.Any(e => e.Contains("not found")))
            return NotFound(result);

        // Other errors için 400 Bad Request
        return BadRequest(result);
    }
}