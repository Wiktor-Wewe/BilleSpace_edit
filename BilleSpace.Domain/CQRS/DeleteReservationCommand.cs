using BilleSpace.Domain.Results;
using BilleSpace.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilleSpace.Domain.CQRS
{
    public class DeleteReservationCommand : IRequest<Result>
    {
        public DeleteReservationCommand(Guid id)
        {
            Id = id;
        }
        public Guid Id { get; set; }
    }

    public class DeleteReservationCommandHandler : IRequestHandler<DeleteReservationCommand, Result>
    {
        private readonly BilleSpaceDbContext _dbContext;
        private readonly ILogger<DeleteReservationCommandHandler> _logger;

        public DeleteReservationCommandHandler(BilleSpaceDbContext dbContext, ILogger<DeleteReservationCommandHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _logger.LogInformation($"[{DateTime.UtcNow}] Object '{nameof(DeleteReservationCommandHandler)}' has been created.");
        }

        public async Task<Result> Handle(DeleteReservationCommand request, CancellationToken cancellationToken)
        {
            var reservation = await _dbContext.Reservations.FirstOrDefaultAsync(x => x.Id == request.Id);

            if(reservation == null)
            {
                _logger.LogError($"[{DateTime.UtcNow}] Can't find reservation (id: {request.Id}).");
                return Result.NotFound(request.Id);
            }

            try
            {
                _dbContext.Reservations.Remove(reservation);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"[{DateTime.UtcNow}] Reservation (id: {request.Id}) was deleted.");
            }
            catch(Exception e)
            {
                _logger.LogError($"[{DateTime.UtcNow}] Error while deleting reservation (id: {request.Id}):");
                _logger.LogError(e.Message);
                return Result.BadRequest("Error while deleting reservation.");
            }

            return Result.Ok();
        }
    }
}
