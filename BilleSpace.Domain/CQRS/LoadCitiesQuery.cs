using BilleSpace.Domain.Results;
using BilleSpace.Infrastructure;
using BilleSpace.Infrastructure.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BilleSpace.Domain.CQRS
{
    public class LoadCitiesQuery : IRequest<Result<List<CityModel>>>
    {
    }

    public class LoadCitiesQueryHandler : IRequestHandler<LoadCitiesQuery, Result<List<CityModel>>>
    {
        private readonly BilleSpaceDbContext _dbContext;
        private readonly ILogger<LoadCitiesQueryHandler> _logger;

        public LoadCitiesQueryHandler(BilleSpaceDbContext dbContext, ILogger<LoadCitiesQueryHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _logger.LogInformation($"[{DateTime.UtcNow}] Object {nameof(LoadCitiesQueryHandler)} has been created.");
        }

        public async Task<Result<List<CityModel>>> Handle(LoadCitiesQuery request, CancellationToken cancellationToken)
        {
            var citiesQuery = _dbContext.Cities
                .Include(cit => cit.Country)
                .Select(cit => new CityModel()
                {
                    Id = cit.Id,
                    Name = cit.Name,
                    Country = new CountryModel()
                    {
                        Name = cit.Country.Name,
                        Id = cit.Country.Id,
                        Symbol = cit.Country.Symbol
                    }
                })
                .AsNoTracking();

            List<CityModel>? cities = null;
            try
            {
                cities = await citiesQuery.ToListAsync(cancellationToken);
            }
            catch (Exception e)
            {
                return Result.BadRequest<List<CityModel>>(new List<string>() { e.Message });
            }
            if (citiesQuery == null)
            {
                _logger.LogInformation($"[{DateTime.UtcNow}]Cant find any cities");
                return Result.NotFound<List<CityModel>>("Cant find any cities" );
            }

            return Result.Ok(cities);
        }
    }
}
