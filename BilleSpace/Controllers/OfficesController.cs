using BilleSpace.Authorization;
using BilleSpace.Domain.CQRS;
using BilleSpace.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using System.Security.Claims;

namespace BilleSpace.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OfficesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OfficesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetOffices([FromQuery]LoadOfficesQuery query)
        {
            var result = _mediator.Send(query);
            return await result.Process();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOffice(Guid id)
        {
            var result = _mediator.Send(new LoadOfficeQuery(id));
            return await result.Process();
        }

        [OnlyReceptionist]
        [HttpPost]
        public async Task<IActionResult> Post(ManageOfficeCommand command)
        {
            command.Id = Guid.Empty;
            command.AuthorEmail = User.FindFirstValue(ClaimTypes.Email);
            return await _mediator.Send(command).Process();
        }

        [OnlyReceptionist]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, ManageOfficeCommand command)
        {
            command.Id = id;
            command.AuthorEmail = User.FindFirstValue(ClaimTypes.Email);
            return await _mediator.Send(command).Process();
        }

        [OnlyReceptionist]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {

            return Ok(await _mediator.Send(new DeleteOfficeCommand(id, User.FindFirstValue(ClaimTypes.Email))));
        }
    }
}
