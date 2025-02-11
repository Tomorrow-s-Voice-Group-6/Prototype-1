using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace TVAttendance.Models
{
    public class Session : IValidatableObject
    {
        public int ID { get; set; }

        [MaxLength(255, ErrorMessage = "Error, cannot have notes more than 255 characters")]
        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Session Date")]
        public DateTime Date { get; set; }

        [Display(Name = "Chapter")]
        [Required]
        public int ChapterID { get; set; }
        public Chapter? Chapter { get; set; }

        public ICollection<SingerSession> SingerSessions { get; set; } = new HashSet<SingerSession>();
        #region Summary

        public string DateFormat => Date.ToShortDateString();
        public string? Summary => $"{Chapter?.City} : {Date}";

        #endregion

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Date < DateTime.Parse("2017-01-01")) //Cannot create session before Tomorrow's Voices began
            {
                yield return new ValidationResult("Session date cannot be before the organization began in 2017-01-01.");
            }
            else if (Date > DateTime.Parse(DateTime.Today.AddDays(1).ToShortDateString())) //Cannot create future session
            {
                yield return new ValidationResult("Session date cannot be in the future.");
            }

        }
    }
}
