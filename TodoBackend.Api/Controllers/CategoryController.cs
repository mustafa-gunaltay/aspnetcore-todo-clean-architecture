using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TodoBackend.Application.Features.TodoCategory.Commands.CreateCategory;
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
    //[SwaggerResponse(StatusCodes.Status200OK, "Created", typeof(Result<int>))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Validation Error Occurred", typeof(Result<int>))]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        if (result.IsSuccess)
            return Ok(result);
        return BadRequest(result);
    }
}
