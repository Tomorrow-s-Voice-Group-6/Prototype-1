using System.ComponentModel.DataAnnotations;

namespace TVAttendance.Models
{
    public class Event : IValidatableObject
    {
        public int ID { get; set; }

        [Display(Name ="Name")]
        [Required]
        public string EventName { get; set; }

        [StringLength(60)]
        [Display(Name = "Street Address")]
        [Required]
        public string EventStreet { get; set; }

        [StringLength(35)]
        [Display(Name = "City")]
        [Required]
        public string EventCity { get; set; }
        
        [StringLength(6)]
        [RegularExpression("^[ABCEGHJ-NPRSTVXY]\\d{1}[ABCEGHJ-NPRSTV-Z]\\d{1}[ABCEGHJ-NPRSTV-Z]\\d{1}", ErrorMessage = "Postal code is in an incorrect format")]
        [Display(Name = "Postal Code")]
        [Required]
        public string EventPostalCode { get; set; }

        [Display(Name = "Province")]
        [Required]
        public Province EventProvince { get; set; }

        [Display(Name = "Start Date")]
        [Required]
        public DateTime EventStart { get; set; }

        [Display(Name = "End Date")]
        [Required]
        public DateTime EventEnd { get; set; }

        public ICollection<Shift>? Shifts { get; set; } = new HashSet<Shift>();

        #region Summary
        public string EventAddress => $"{EventStreet}, {EventCity}, {EventPostalCode} - {EventProvince}";

        [Display(Name = "Event Date")]
        public string EventDate => $"{EventStart.ToShortDateString()} - {EventEnd.ToShortDateString()}";

        [Display(Name = "Event Time")]
        public string EventTime => $"{EventStart.ToShortTimeString()} - {EventEnd.ToShortTimeString()}";
        #endregion
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EventStart.CompareTo(DateTime.Now) < 0)
            {
                yield return new ValidationResult("The event cannot start in the past", ["EventStart"]);
            }
            if (EventEnd.CompareTo(DateTime.Now) < 0)
            {
                yield return new ValidationResult("The event cannot end in the past", ["EventEnd"]);
            }
            if (EventStart.CompareTo(EventEnd) > 0)
            {
                yield return new ValidationResult("The event start date cannot be after the end date", ["EventStart"]);
            }
            if (EventEnd.CompareTo(EventStart) < 0)
            {
                yield return new ValidationResult("The event end date cannot be before the start date", ["EventEnd"]);
            }
        }
    }
}
