using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using TVAttendance.Data;

namespace TVAttendance.Models
{
    public class VolunteerEvent : IValidatableObject
    {
        public int EventID { get; set; }
        public Event? Event { get; set; }

        public int? VolunteerID { get; set; }
        public Volunteer? Volunteer { get; set; }

        public bool ShiftAttended { get; set; }

        [Required]
        public DateTime ShiftStart { get; set; }
        [Required]
<<<<<<< Updated upstream:TVAttendance/TVAttendance/Models/VolunteerEvent.cs
        public DateTime ShiftEnd{ get; set; }
        public string? NonAttendanceNote { get; set; }
=======
        public DateTime ShiftEnd { get; set; }

        public ICollection<ShiftVolunteer> ShiftVolunteers { get; set; } = new HashSet<ShiftVolunteer>();
>>>>>>> Stashed changes:TVAttendance/TVAttendance/Models/Shift.cs

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var _context = (TomorrowsVoiceContext)validationContext.GetService(typeof(TomorrowsVoiceContext));

            // Ensure Event is loaded
            var eventData = _context.Events.AsNoTracking().FirstOrDefault(e => e.ID == EventID);

            if (eventData == null)
            {
                yield return new ValidationResult("Selected Event does not exist.", new[] { "EventID" });
                yield break;
            }

            if (ShiftStart < eventData.EventStart)
            {
                yield return new ValidationResult("Shift can't start before the event starts.", new[] { "ShiftStart" });
            }

            if (ShiftEnd > eventData.EventEnd)
            {
                yield return new ValidationResult("Shift can't end after the event ends.", new[] { "ShiftEnd" });
            }
        }
    }
}
