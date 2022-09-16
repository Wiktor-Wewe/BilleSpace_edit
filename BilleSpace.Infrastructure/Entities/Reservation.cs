namespace BilleSpace.Infrastructure.Entities
{
    public class Reservation
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }

        public Guid OfficeId { get; set; }
        public Office Office { get; set; }

        public Guid OfficeZoneId { get; set; }
        public OfficeZone OfficeZone { get; set; }
        public string OfficeDesk { get; set; }

        public Guid? ParkingZoneId { get; set; }
        public ParkingZone? ParkingZone { get; set; }
        public string? ParkingSpace { get; set; }

        public string UserEmail { get; set; }
    }
}
