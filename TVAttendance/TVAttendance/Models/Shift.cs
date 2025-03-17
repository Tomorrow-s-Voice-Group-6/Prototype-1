using SQLitePCL;
using System.ComponentModel.DataAnnotations;

namespace TVAttendance.Models
{
    public class Shift : IValidatableObject
    {
        [Key]
        public int ID { get; set; }
        public int EventID { get; set; }
        public Event? Event { get; set; }

        [Required]
        [Display(Name = "Shift Date")]
        public DateOnly ShiftDate { get; set; }

        [Required]
        [Display(Name = "Shift Start")]
        public TimeOnly ShiftStart { get; set; }

        [Required]
        [Display(Name = "Shift End")]
        public TimeOnly ShiftEnd { get; set; }

        public ICollection<ShiftVolunteer> ShiftVolunteers { get; set; } = new HashSet<ShiftVolunteer>();

        [Display(Name = "Shift Time")]
        public string ShiftRange => $"{ShiftStart.ToShortTimeString()} - {ShiftEnd.ToShortTimeString()}";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Event != null)
            {
                if (ShiftDate.ToDateTime(TimeOnly.MinValue).Date.CompareTo(Event.EventStart) < 0 
                    || ShiftDate.ToDateTime(TimeOnly.MinValue).Date.CompareTo(Event.EventEnd) > 0)
                {
                    yield return new ValidationResult("Shift must be within the Event Start and End Time", ["ShiftDate"]);
                }
            }
            
            if (ShiftEnd.CompareTo(ShiftStart) < 0)
            {
                yield return new ValidationResult("Shift can't end before it starts.", ["ShiftEnd"]);
            }
            if (ShiftStart.CompareTo(ShiftEnd) > 0)
            {
                yield return new ValidationResult("Shift can't start after it ends.", ["ShiftStart"]);
            }
        }
    }
}
