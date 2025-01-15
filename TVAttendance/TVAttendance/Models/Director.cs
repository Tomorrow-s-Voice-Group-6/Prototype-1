using System.ComponentModel.DataAnnotations;

namespace TVAttendance.Models
{
    public class Director
    {
        [Key]
        public int ID { get; set; }
        [Display(Name = "First Name")]
        [MaxLength(55, ErrorMessage = "Error: First name cannot be more than 50 characters")]
        [Required]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [MaxLength(100, ErrorMessage = "Error: Last name cannot be more than 100 characters")]
        [Required]
        public string LastName { get; set; }

        [Display(Name = "Date of Birth")]
        [Required]
        public DateTime DOB { get; set; }
        [Display(Name = "Hire Date")]
        public DateOnly EmploymentDate { get; set; } //Note DATEONLY

        [Display(Name = "Address")]
        [MaxLength(255)]
        public string Address { get; set; }

        [Display(Name = "Email")]
        [Required]
        [MaxLength(255)]
        public string Email { get; set; }

        [Display(Name = "Phone Number")]
        [Required]
        public string Phone { get; set; }

        [Display(Name = "Status")]
        [Required]
        public bool Status { get; set; }
        //public int ChapterID { get; set; }
        //public Chapter chapter { get; set; } = new HashSet<Chapter>();

        #region Summary
        public string FullName => $"{FirstName} {LastName}";
        #endregion
    }
}
