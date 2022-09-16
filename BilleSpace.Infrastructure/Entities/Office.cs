namespace BilleSpace.Infrastructure.Entities
{
    public class Office
    {
        public Guid Id { get; set; }
        public string Address { get; set; }

        public string PostCode { get; set; }
        public string? OfficeMapUrl { get; set; }


        public Guid CityId { get; set; }
        public City City { get; set; }

        public List<OfficeZone> OfficeZones { get; set; } = new List<OfficeZone>();
        public List<ParkingZone>? ParkingZones { get; set; } = new List<ParkingZone>();

        public string AuthorEmail { get; set; }
    }
}