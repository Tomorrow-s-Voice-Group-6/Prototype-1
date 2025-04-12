using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

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

        [Display(Name ="Status")]
        [Required]
        public bool EventOpen { get; set; }

        public ICollection<Shift>? Shifts { get; set; } = new HashSet<Shift>();

        //AI generated method
        //Prompt: is there a way i can make a string out of a string like Prince Edward Island, and only take the
        //capital letters out of the string. If the string is only 1 word, then take first 2 letters and capitalize it in c#
        public string ProvinceAbbreviation(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            //Get all Captial/Uppercase letters
            var matches = Regex.Matches(text, "[A-Z]");

            //Special cases for provinces with different abbreviations
            if (text == "NewfoundlandAndLabrador")
                return "NL";
            else if (text == "Saskatchewan")
                return "SK";
            else if (text == "Quebec")
                return "QC";
            else if (text == "Yukon")
                return "YT";
            else if (text == "Alberta")
                return "AB";

            if (matches.Count > 1)
            {
                return string.Concat(matches.Select(m => m.Value)).ToUpper();
            }
            else
            {
                // If only one capital letter exists, take the first two characters
                return text.Length >= 2 ? text.Substring(0, 2).ToUpper() : text.ToUpper();
            }
        }

        #region Summary
        public string PostalCodeFormatted => $"{EventPostalCode.Substring(0, 3)}-{EventPostalCode.Substring(3)}";
        public string EventAddress => $"{EventStreet}, {EventCity}, {EventProvince} - {PostalCodeFormatted}";
        public string EventAddressDashboard => $"{EventStreet}, {EventCity} - {ProvinceAbbreviation(EventProvince.ToString())}";

        [Display(Name = "Event Period")]
        public string EventDate => $"{EventStart.ToShortDateString()} - {EventEnd.ToShortDateString()}";

        [Display(Name = "Event Time")]
        public string EventTime => $"{EventStart.ToShortTimeString()} - {EventEnd.ToShortTimeString()}";
        public string EventStartDate => EventStart.ToShortDateString();
        public string EventEndDate => EventEnd.ToShortDateString();
        #endregion

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
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
