using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace TVAttendance.Models
{
    public class Chapter
    {
        [Key]
        public int ChapterID { get; set; }

        [MaxLength(100)]
        [Required]
        public string ChapterCity { get; set; }

        [MaxLength(255)]
        public string ChapterAddress { get; set; }

        // Foreign Key to Director
        // Each Chapter is associated with one Director
        [Required]
        public int DirectorID { get; set; }

        // Navigation to Director
        public Director Director { get; set; }

        // Navigation for 1:Many relationships
        public ICollection<ChoirProgram> Programs { get; set; }
        public ICollection<Volunteer> Volunteers { get; set; }
        public ICollection<Singer> Singers { get; set; }
    }
}
