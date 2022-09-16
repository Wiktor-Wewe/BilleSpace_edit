using BilleSpace.Domain.CQRS;
using BilleSpace.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BilleSpace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromQuery]LoginQuery query)
        {
            var result = _mediator.Send(query);
            return await result.Process();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromQuery] RegisterCommand command)
        {
            var result = _mediator.Send(command);
            return await result.Process();
        }
    }
}
