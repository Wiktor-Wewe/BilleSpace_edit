namespace BilleSpace.Infrastructure.Models
{
    public class ReservationModel : BaseModel
    {
        public DateTime Date { get; set; }

        public OfficeModel Office { get; set; }

        public OfficeZoneModel OfficeZone { get; set; }
        public string OfficeDesk { get; set; }

        public ParkingZoneModel? ParkingZone { get; set; }
        public string? ParkingSpace { get; set; }
    }
}
