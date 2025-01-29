using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TVAttendance.Models
{
    public class Session : IValidatableObject
    {
        private static readonly DateTime OrgStartDate = new DateTime(2017, 1, 1);

        public int ID { get; set; }

        [MaxLength(255, ErrorMessage = "Error, cannot have notes more than 255 characters")]
        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Date of Program")]
        public DateOnly Date { get; set; }

        [Required]
        [Display(Name = "Chapter")]
        public int ChapterID { get; set; }
        public Chapter? Chapter { get; set; }

        public ICollection<SingerSession> SingerSessions { get; set; } = new HashSet<SingerSession>();

        // Read-Only Summary
        public string? Summary => $"{Chapter?.City} - {Date:yyyy-MM-dd}";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
<<<<<<< HEAD
            if (Date < DateOnly.Parse("2017-01-01")) //Cannot create session before Tomorrow's Voices began
=======
            if (Date < OrgStartDate)
>>>>>>> 061c279 (Updated _Layout.cshtml, SessionController, and Home/Index.cshtml)
            {
                yield return new ValidationResult(
                    $"Error: Session date cannot be before the organization opened on {OrgStartDate:yyyy-MM-dd}.",
                    new[] { nameof(Date) }
                );
            }
<<<<<<< HEAD
            else if (Date > DateOnly.Parse(DateTime.Today.AddDays(1).ToShortDateString())) //Cannot create future session
=======
            else if (Date > DateTime.Today.AddDays(1))
>>>>>>> 061c279 (Updated _Layout.cshtml, SessionController, and Home/Index.cshtml)
            {
                yield return new ValidationResult(
                    "Error: Session date cannot be more than 1 day in the future.",
                    new[] { nameof(Date) }
                );
            }
        }
    }
}
