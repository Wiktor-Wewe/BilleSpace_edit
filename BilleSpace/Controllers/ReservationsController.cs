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
    public class ReservationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReservationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> PostReservation(ManageReservationCommand command)
        {
            command.UserEmail = User.FindFirstValue(ClaimTypes.Email);
            var result = _mediator.Send(command);
            return await result.Process();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservation([FromRoute]Guid id, ManageReservationCommand command)
        {
            command.Id = id;
            command.UserEmail = User.FindFirstValue(ClaimTypes.Email);
            var result = _mediator.Send(command);
            return await result.Process();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(Guid id)
        {
            var result = _mediator.Send(new DeleteReservationCommand(id));
            return await result.Process();
        }

        [HttpGet]
        public async Task<IActionResult> GetReservations([FromQuery] LoadReservationsQuery query)
        {
            var result = _mediator.Send(query);
            return await result.Process();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservationDetails(Guid id)
        {
            var result = _mediator.Send(new LoadReservationDetailsQuery(id));
            return await result.Process();
        }
    }
}
