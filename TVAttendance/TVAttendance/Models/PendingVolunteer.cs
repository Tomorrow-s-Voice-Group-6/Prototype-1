using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TVAttendance.Models
{
    public class PendingVolunteer : IValidatableObject
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
        [MinLength(10, ErrorMessage = "Please ensure you filled the entire phone number! Example: 000-000-0000")]
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
        public DateTime RegisterDate { get; set; }

        //public string UserId { get; set; } // Identity FK

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Age: between 8 and 80 years
            DateTime minAge = DateTime.Now.AddYears(-8);
            DateTime maxAge = DateTime.Now.AddYears(-80);

            if (DOB > minAge)
            {
                yield return new ValidationResult("Volunteer must be at least 8 years old.", new[] { nameof(DOB) });
            }
            else if (DOB >= DateTime.Now)
            {
                yield return new ValidationResult("Date of birth cannot be in the future.", new[] { nameof(DOB) });
            }
            else if (DOB < maxAge)
            {
                yield return new ValidationResult("Volunteer cannot be older than 80 years.", new[] { nameof(DOB) });
            }

            // No validation for RegisterDate here – deferred until approval
        }
    }
}
