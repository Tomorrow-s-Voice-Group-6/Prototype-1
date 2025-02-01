using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVAttendance.Models
{
    public class Singer
    {
        public int ID { get; set; }

        [Display(Name = "First Name")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Required]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Required]
        public string LastName { get; set; }

        [Display(Name = "Date of Birth")]
        [Required]
        public DateTime DOB { get; set; }

        [Display(Name = "Address")]
        [MaxLength(255)]
        public string Address { get; set; }

        [Display(Name = "Active")]
        public bool Status { get; set; }

        [Display(Name = "Register Date")]
        [Required]
        public DateTime RegisterDate { get; set; }

        //thinking about removing emergency contact information for simplicity sake
        [Display(Name = "Emergency Contact First Name")]
        [MaxLength(50, ErrorMessage = "Emergency contact first name cannot exceed 50 characters")]
        public string EmergencyContactFirstName { get; set; }

        [Display(Name = "Emergency Contact Last Name")]
        [MaxLength(50, ErrorMessage = "Emergency contact last name cannot exceed 50 characters")]
        public string EmergencyContactLastName { get; set; }

        [Display(Name = "Emergency Contact Phone")]
        [RegularExpression("^\\d{10}", ErrorMessage = "Phone number must be 10 digits in length.")]
        [StringLength(10)]
        [Required]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string EmergencyContactPhone { get; set; }

        [Display(Name = "Chapter")]
        [Required]
        public int ChapterID { get; set; }
        public Chapter? Chapter { get; set; }

        public ICollection<SingerSession> SingerSessions { get; set; } = new HashSet<SingerSession>();

        #region Summary
        [Display(Name = "Name")]
        public string FullName => $"{FirstName} {LastName}";
        public string Summary => $"{FullName} - {DOB.ToShortDateString()}";

        [Display(Name = "Emergency Contact")]
        public string EmergFullName => $"{EmergencyContactFirstName} {EmergencyContactLastName}";
        #endregion

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.DOB.CompareTo(DateTime.Now) == 1)//if CompareTo() returns 1, then the DOB is later than now
            {
                yield return new ValidationResult("Singer date of birth cannot be in the future.", ["DOB"]);
            }

            if (this.RegisterDate > this.DOB)
            {
                yield return new ValidationResult("Singer cannot register in the future.", ["RegisterDate"]);
            }
        }
    }
}
