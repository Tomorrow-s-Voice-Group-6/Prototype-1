using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVAttendance.Models
{
    public class Director
    {
        [Key]
        public int DirectorID { get; set; }

        [MaxLength(50)]
        [Required]
        public string DirectorFirstName { get; set; }

        [MaxLength(50)]
        [Required]
        public string DirectorLastName { get; set; }

        public DateTime DirectorDOB { get; set; }
        public DateTime DirectorEmploymentDate { get; set; }

        [MaxLength(255)]
        public string DirectorAddress { get; set; }

        [MaxLength(255)]
        public string DirectorEmail { get; set; }

        [MaxLength(20)]
        public string DirectorPhone { get; set; }

        public bool DirectorStatus { get; set; }

        // If you want a one-to-one or one-to-many with Chapter, 
        // define navigation from the Chapter side as needed.
        // This model does NOT need a ChapterID if the FK is stored in Chapter.
    }
}
