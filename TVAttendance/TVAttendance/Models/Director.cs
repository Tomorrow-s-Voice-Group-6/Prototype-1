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

        [Display(Name = "Date of Birth")]
        public DateTime? DOB { get; set; }

        [Required]
        [Display(Name = "Date Of Registry")]
        public DateTime HireDate { get; set; }

        [MaxLength(255)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Required]
        [MaxLength(255)]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required]
        [MinLength(10, ErrorMessage = "Please ensure the number is 10 digits")]
        [StringLength(10)]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Error, invalid phone number format")]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; } = true;

        //public int? ChapterID { get; set; }
        //public Chapter? Chapter { get; set; }

        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();


        #region Summary
        [Display(Name ="Name")]
        public string FullName => $"{FirstName} {LastName}";
        
        [Display(Name = "E-Contact Phone")]
        public string DisplayPhone => $"({Phone.Substring(0, 3)}) {Phone.Substring(3, 3)}-{Phone.Substring(6)}";
        #endregion
    }
}
