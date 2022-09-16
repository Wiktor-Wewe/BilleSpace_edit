using BilleSpace.Infrastructure.Entities;

namespace BilleSpace.Infrastructure.Models
{
    public class OfficeModel : BaseModel
    {
        public CityModel City { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }
        public IEnumerable<OfficeZoneModel> OfficeZones { get; set; }
        public IEnumerable<ParkingZoneModel>? ParkingZones { get; set; }
        public string? OfficeMapUrl { get; set; }

        public static OfficeModel ToViewModel(Office office)
        {
            var viewModel = new OfficeModel()
            {
                Id = office.Id,
                City = new CityModel
                {
                    Id = office.CityId,
                    Name = office.City.Name,
                    Country = new CountryModel()
                    {
                        Id = office.City.CountryId,
                        Name = office.City.Country.Name,
                        Symbol = office.City.Country.Symbol,
                    }
                },
                Address = office.Address,
                PostCode = office.PostCode,
                OfficeMapUrl = office.OfficeMapUrl,
                OfficeZones = office.OfficeZones.Select(x => new OfficeZoneModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Desks = x.Desks
                }),
                ParkingZones = office.ParkingZones.Select(x => new ParkingZoneModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Spaces = x.Spaces
                }),
            };

            return viewModel;
        }
    }
}
