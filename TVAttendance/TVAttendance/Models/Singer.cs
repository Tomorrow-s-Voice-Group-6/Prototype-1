using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVAttendance.Models
{
    public class Singer
    {
        [Key]
        public int SingerID { get; set; }

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

        [Display(Name = "Status")]
        public bool Status { get; set; } // Active status of the singer

        [Display(Name = "Register Date")]
        [Required]
        public DateTime RegisterDate { get; set; }

        [Display(Name = "Emergency Contact First Name")]
        [MaxLength(50, ErrorMessage = "Emergency contact first name cannot exceed 50 characters")]
        public string EmergencyContactFirstName { get; set; }

        [Display(Name = "Emergency Contact Last Name")]
        [MaxLength(50, ErrorMessage = "Emergency contact last name cannot exceed 50 characters")]
        public string EmergencyContactLastName { get; set; }

        [Display(Name = "Emergency Contact Phone")]
        [MaxLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string EmergencyContactPhone { get; set; }

        [Display(Name = "Email")]
        [MaxLength(255)]
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        // Foreign Key Relationship with Chapter
        [Required]
        public int ChapterID { get; set; }
        public Chapter Chapter { get; set; }

        // Many-to-Many Relationship with Program
        public ICollection<SingerProgram> SingerPrograms { get; set; }

        // Computed Property for Full Name
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}
