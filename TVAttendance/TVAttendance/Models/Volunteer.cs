using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVAttendance.Models
{
    public class Volunteer : IValidatableObject
    {
        public int ID { get; set; }

        [Display(Name = "First Name")]
        [MaxLength(55, ErrorMessage = "First name cannot exceed 55 characters")]
        [Required]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        [Required]
        public string LastName { get; set; }

        [Display(Name = "Phone Number")]
        [Required]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string Phone { get; set; }

        [Display(Name = "Email")]
        [MaxLength(255)]
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Display(Name = "Date of Birth")]
        [Required]
        public DateTime DOB { get; set; }

        [Display(Name = "Register Date")]
        [Required]
        public DateTime RegisterDate { get; set; } 
        public ICollection<VolunteerEvent> VolunteerEvents { get; set; } = new HashSet<VolunteerEvent>();

        #region Summary
        [Display(Name = "Volunteer")]
        public string FullName => $"{FirstName} {LastName}";
        public string DOBFormatted => DOB.ToShortDateString();
        public string RegisterFormatted => RegisterDate.ToShortDateString();

        #endregion
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //Min age is 13
            DateTime minAge = DateTime.Parse($"{DateTime.Now.AddYears(-13)}");
            if (DOB < minAge)
            {
                yield return new ValidationResult("Volunteer date of birth cannot less than 13");
            }
            else if (DOB > DateTime.Now)
            {
                yield return new ValidationResult("Volunteer date of birth cannot be in the future");
            }

            
        }
    }
}
