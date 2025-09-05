using MediatR;
using Microsoft.AspNetCore.Mvc;
using TodoBackend.Application.Features.TodoUser.Commands.CreateUser;
using TodoBackend.Application.Features.TodoUser.Commands.UpdateUser;
using TodoBackend.Application.Features.TodoUser.Commands.DeleteUser;
using TodoBackend.Application.Features.TodoUser.Queries.GetAllUsers;
using TodoBackend.Application.Features.TodoUser.Queries.GetUserById;
using TodoBackend.Application.Features.TodoUser.Queries.ValidateUserCredentials;
using TodoBackend.Application.Features.BuildingBlocks;

namespace TodoBackend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    //[SwaggerOperation("Create User")]
    //[SwaggerResponse(StatusCodes.Status201Created, "Created", typeof(Result<int>))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result<int>))]
    //[SwaggerResponse(StatusCodes.Status409Conflict, "Email already exists", typeof(Result<int>))]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        if (result.IsSuccess)
        {
            // 201 Created - Yeni kaynak oluşturuldu, Location header ile kaynak URI'sini ver
            return Created($"/api/user/{result.Value}", result);
        }
        
        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);
            
        // Business rule violations (örn: duplicate email) için 409 Conflict
        return Conflict(result);
    }

    [HttpGet]
    //[SwaggerOperation("Get All Users")]
    //[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(Result<IReadOnlyList<TodoBackend.Application.ViewModels.UserViewModel>>))]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllUsersQuery(), cancellationToken);
        if (result.IsSuccess)
        {
            // 200 OK - Sorgu başarılı ve data döndürüyoruz
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("{id}")]
    //[SwaggerOperation("Get User By Id")]
    //[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(Result<TodoBackend.Application.ViewModels.UserViewModel>))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid UserId", typeof(Result<TodoBackend.Application.ViewModels.UserViewModel>))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "User not found", typeof(Result<TodoBackend.Application.ViewModels.UserViewModel>))]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
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
    //[SwaggerOperation("Update User")]
    //[SwaggerResponse(StatusCodes.Status204NoContent, "Updated successfully")]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "User not found", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status409Conflict, "Email already exists", typeof(Result))]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // Route'dan gelen id ile request'teki id'yi senkronize et
        var command = request with { UserId = id };
        
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsSuccess)
        {
            // 204 No Content - Güncelleme başarılı ama dönecek gövde yok
            return NoContent();
        }
        
        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);
            
        // User not found için 404 Not Found
        if (result.Errors.Any(e => e.Contains("not found")))
            return NotFound(result);
            
        // Business rule violations (örn: duplicate email) için 409 Conflict
        return Conflict(result);
    }

    [HttpDelete("{id}")]
    //[SwaggerOperation("Delete User")]
    //[SwaggerResponse(StatusCodes.Status204NoContent, "Deleted successfully")]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status404NotFound, "User not found", typeof(Result))]
    //[SwaggerResponse(StatusCodes.Status409Conflict, "Cannot delete user with active tasks", typeof(Result))]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteUserCommand(id), cancellationToken);
        if (result.IsSuccess)
        {
            // 204 No Content - Silme başarılı (soft delete), dönecek gövde yok
            return NoContent();
        }
        
        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);
            
        // User not found için 404 Not Found
        if (result.Errors.Any(e => e.Contains("not found")))
            return NotFound(result);
            
        // Business rule violations (örn: active tasks var) için 409 Conflict
        return Conflict(result);
    }

    [HttpPost("validate-credentials")]
    //[SwaggerOperation("Validate User Credentials")]
    //[SwaggerResponse(StatusCodes.Status200OK, "Validation completed", typeof(Result<bool>))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result<bool>))]
    public async Task<IActionResult> ValidateCredentials([FromBody] ValidateUserCredentialsQuery request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        if (result.IsSuccess)
        {
            // 200 OK - Credential validation başarılı (true/false döner)
            return Ok(result);
        }
        
        // Validation errors için 400 Bad Request
        if (result.HasValidationErrors)
            return BadRequest(result);
            
        // Diğer hatalar için 400 Bad Request
        return BadRequest(result);
    }
}
