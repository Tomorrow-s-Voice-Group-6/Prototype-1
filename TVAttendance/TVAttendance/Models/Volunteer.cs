using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVAttendance.Models
{
    public class Volunteer
    {
        [Key]
        public int VolunteerID { get; set; }

        [MaxLength(50)]
        [Required]
        public string VolunteerFirstName { get; set; }

        [MaxLength(50)]
        [Required]
        public string VolunteerLastName { get; set; }

        [MaxLength(20)]
        public string VolunteerPhone { get; set; }

        [MaxLength(255)]
        [Required]
        public string VolunteerEmail { get; set; }

        public DateTime VolunteerDOB { get; set; }
        public DateTime VolunteerRegisterDate { get; set; }

        // Foreign Key
        [Required]
        public int ChapterID { get; set; }

        // Navigation Property
        public Chapter Chapter { get; set; }
    }
}
