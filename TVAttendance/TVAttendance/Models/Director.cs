using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVAttendance.Models
{
    public class Director
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

        [DataType(DataType.PhoneNumber, ErrorMessage = "Error, invalid phone number format")]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; }

        #region Summary
        public string FullName => $"{FirstName} {LastName}";
        #endregion
    }
}
