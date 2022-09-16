using BilleSpace.Domain.Results;
using BilleSpace.Infrastructure;
using BilleSpace.Infrastructure.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BilleSpace.Domain.CQRS
{
    public class LoadCountriesQuery : IRequest<Result<List<CountryModel>>>
    {
    }

    public class LoadCountriesQueryHandler : IRequestHandler<LoadCountriesQuery, Result<List<CountryModel>>>
    {
        private readonly BilleSpaceDbContext _dbContext;
        private readonly ILogger<LoadCountriesQueryHandler> _logger;

        public LoadCountriesQueryHandler(BilleSpaceDbContext dbContext, ILogger<LoadCountriesQueryHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _logger.LogInformation($"[{DateTime.UtcNow}] Object {nameof(LoadCountriesQueryHandler)} has been created.");
        }

        public async Task<Result<List<CountryModel>>> Handle(LoadCountriesQuery request, CancellationToken cancellationToken)
        {


            var countryQuery = _dbContext.Countries
                .Select(cou => new CountryModel()
                {
                    Id = cou.Id,
                    Name = cou.Name,
                    Symbol = cou.Symbol
                })
                .AsNoTracking();

            List<CountryModel>? country = null;
            try
            {
                country = await countryQuery.ToListAsync(cancellationToken);
            }
            catch (Exception e)
            {
                return Result.BadRequest<List<CountryModel>>(new List<string>() { e.Message });
            }
            if (countryQuery == null)
            {
                _logger.LogInformation($"[{DateTime.UtcNow}]Cant find any country");
                return Result.NotFound<List<CountryModel>>("Cant find any country");
            }

            return Result.Ok(country);
        }

    }
}
