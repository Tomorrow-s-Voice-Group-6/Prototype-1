using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVAttendance.Models
{
    public class Volunteer
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
        [DataType(DataType.PhoneNumber)]
        [Required]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string Phone { get; set; }

        [Display(Name = "Email")]
        [MaxLength(255)]
        [DataType(DataType.EmailAddress)]
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Display(Name = "Date of Birth")]
        [Required]
        public DateTime DOB { get; set; }

        [Display(Name = "Register Date")]
        [Required]
        public DateOnly RegisterDate { get; set; } //Note DateOnly
        public int ChapterID { get; set; }
        public Chapter chapter { get; set; }

        #region Summary
        public string FullName => $"{FirstName} {LastName}";
        #endregion
    }
}
