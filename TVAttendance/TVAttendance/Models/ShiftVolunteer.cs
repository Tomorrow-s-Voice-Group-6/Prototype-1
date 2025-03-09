namespace TVAttendance.Models
{
    public class ShiftVolunteer
    {
        public int ShiftID { get; set; }
        public Shift? Shift { get; set; }
        public int VolunteerID { get; set; }
        public Volunteer? Volunteer { get; set; }


        public DateTime? ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }
        public bool NonAttendance { get; set; } = true;
        public string? Note { get; set; }
    }
}
