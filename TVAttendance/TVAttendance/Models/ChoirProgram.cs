using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace TVAttendance.Models
{
    public class ChoirProgram
    {
        [Key]
        public int ProgramID { get; set; }

        public string notes { get; set; }
        public DateTime date { get; set; }

        // Foreign Key - the Chapter that this Program belongs to
        [Required]
        public int ChapterID { get; set; }
        public Chapter Chapter { get; set; }

        // For Many-to-Many with Singer
        public ICollection<SingerProgram> SingerPrograms { get; set; }
    }
}
