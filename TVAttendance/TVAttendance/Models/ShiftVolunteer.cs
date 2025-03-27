using System.ComponentModel.DataAnnotations;

namespace TVAttendance.Models
{
    public class ShiftVolunteer
    {
        public int ShiftID { get; set; }
        public Shift? Shift { get; set; }
        public int? VolunteerID { get; set; }
        public Volunteer? Volunteer { get; set; }

        public DateTime? ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }

        [Display(Name ="Attendance Status")]
        public bool? NonAttendance { get; set; } = null;

        [Display(Name = "Reason")]
        public AttendanceReason? AttendanceReason { get; set; }
    }
}
