using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVAttendance.Models
{
    public class Volunteer
    {
        [Key]
        public int VolunteerID { get; set; } // Primary key

        [Display(Name = "First Name")]
        [MaxLength(55, ErrorMessage = "First name cannot exceed 55 characters")]
        [Required]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        [Required]
        public string LastName { get; set; }

        [Display(Name = "Contact Number")]
        [MaxLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
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
        public DateTime RegisterDate { get; set; } // Register date for the volunteer

        // Foreign Key
        [Required]
        public int ChapterID { get; set; }

        // Navigation Property
        public Chapter Chapter { get; set; }

        // Computed Property for Full Name
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}
