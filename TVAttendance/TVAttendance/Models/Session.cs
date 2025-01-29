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
            if (Date.ToDateTime(TimeOnly.MinValue) < OrgStartDate)
            {
                yield return new ValidationResult(
                    $"Error: Session date cannot be before the organization opened on {OrgStartDate:yyyy-MM-dd}.",
                    new[] { nameof(Date) }
                );
            }
            else if (Date.ToDateTime(TimeOnly.MinValue) > DateTime.Today.AddDays(1))
            {
                yield return new ValidationResult(
                    "Error: Session date cannot be more than 1 day in the future.",
                    new[] { nameof(Date) }
                );
            }
        }
    }
}
