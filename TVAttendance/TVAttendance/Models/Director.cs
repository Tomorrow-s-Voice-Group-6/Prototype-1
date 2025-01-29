using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TVAttendance.Models
{
    public class Director : IValidatableObject
    {
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Date of Birth")]
        public DateTime DOB { get; set; }

        [Required]
        [Display(Name = "Hire Date")]
        public DateTime HireDate { get; set; }

        [MaxLength(255)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Required]
        [MaxLength(255)]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; }

        // Summary Fields
        [Display(Name = "Name")]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "E-Contact Phone")]
        public string DisplayPhone
        {
            get
            {
                // Return empty or some default if null or too short
                if (string.IsNullOrEmpty(Phone) || Phone.Length < 10)
                    return Phone ?? "";

                return $"({Phone.Substring(0, 3)}) {Phone.Substring(3, 3)}-{Phone.Substring(6)}";
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Ensure DOB < HireDate
            if (DOB >= HireDate)
            {
                yield return new ValidationResult(
                    "Date of Birth cannot be on or after the Hire Date.",
                    new[] { nameof(DOB), nameof(HireDate) }
                );
            }

            // Ensure Director is at least 18 at Hire
            int ageAtHire = HireDate.Year - DOB.Year;
            if (DOB.Date > HireDate.AddYears(-ageAtHire))
            {
                ageAtHire--;
            }
            if (ageAtHire < 18)
            {
                yield return new ValidationResult(
                    "Director must be at least 18 years old at time of hire.",
                    new[] { nameof(DOB), nameof(HireDate) }
                );
            }
        }
    }
}
