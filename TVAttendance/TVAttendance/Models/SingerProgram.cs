using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVAttendance.Models
{
    public class SingerProgram
    {
        [Key]
        public int SingerProgramID { get; set; } // Primary Key

        // Foreign Keys
        [Required]
        [Display(Name = "Singer ID")]
        public int SingerID { get; set; }

        [Required]
        [Display(Name = "Program ID")]
        public int ProgramID { get; set; }

        // Navigation Properties
        public Singer Singer { get; set; }
        public ChoirProgram Program { get; set; }
    }
}
