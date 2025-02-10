using System.ComponentModel.DataAnnotations;

namespace TVAttendance.Models
{
    public class VolunteerEvent : IValidatableObject
    {
        public int EventID { get; set; }
        public Event Event { get; set; }

        public int? VolunteerID { get; set; }
        public Volunteer? Volunteer { get; set; }

        public bool ShiftAttended { get; set; }

        [Required]
        public DateTime ShiftStart { get; set; }
        [Required]
        public DateTime ShiftEnd{ get; set; }
        public string? NonAttendanceNote { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(ShiftStart.CompareTo(Event.EventStart) < 0)
            {
                yield return new ValidationResult("Shift can't start before the event started", ["ShiftStart"]);
            }
            if (ShiftEnd.CompareTo(Event.EventEnd) > 0)
            {
                yield return new ValidationResult("Shift can't end after the event ended", ["ShiftEnd"]);
            }
        }
    }
}
