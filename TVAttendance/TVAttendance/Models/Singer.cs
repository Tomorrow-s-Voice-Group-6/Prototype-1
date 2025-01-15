using System.ComponentModel.DataAnnotations;

namespace TVAttendance.Models
{
    public class Singer
    {
        [Key]
        public int ID { get; set; }
        [Display(Name = "First Name")]
        [MaxLength(55)]
        [Required]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [MaxLength(100)]
        [Required]
        public string LastName { get; set; }

        [Display(Name = "Date of Birth")]
        [Required]
        public DateTime DOB { get; set; }

        [Display(Name = "Email")]
        [Required]
        [MaxLength(255)]
        public string Email { get; set; }

        [Display(Name = "Phone Number")]
        [Required]
        public string Phone { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; }

        [Display(Name = "Register Date")]
        public DateOnly RegisterDate { get; set; }
        [Display(Name = "Emergency Contact First Name")]
        public string EmergencyContactFirstName { get; set; }
        [Display(Name = "Emergency Contact Last Name")]
        public string EmergencyContactLastName { get; set; }
        [Display(Name = "Emergency Contact Phone")]
        public string EmergencyContactPhone { get; set; }

        /*public Chapter ChapterID { get; set; } *///Foreign key
    }
}
