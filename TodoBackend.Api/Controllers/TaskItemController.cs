using MediatR;
using Microsoft.AspNetCore.Mvc;
using TodoBackend.Application.Features.TodoTaskItem.Commands.CreateTaskItem;
using TodoBackend.Application.Features.TodoTaskItem.Commands.UpdateTaskItem;
using TodoBackend.Application.Features.TodoTaskItem.Commands.DeleteTaskItem;
using TodoBackend.Application.Features.BuildingBlocks;

namespace TodoBackend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskItemController : ControllerBase
{
    private readonly IMediator _mediator;

    public TaskItemController(IMediator mediator)
    {
        _mediator = mediator;
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
            // 201 Created - Yeni kaynak olu?turuldu, Location header ile kaynak URI'sini ver
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
            // 200 OK - Güncelleme ba?ar?l?, success message ile birlikte
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
            // 200 OK - Silme ba?ar?l?, success message ile birlikte
            return Ok(result);
        }
        
        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);
            
        // Task not found için 404 Not Found
        if (result.Errors.Any(e => e.Contains("not found")))
            return NotFound(result);
            
        // Di?er hatalar için 400 Bad Request
        return BadRequest(result);
    }
}