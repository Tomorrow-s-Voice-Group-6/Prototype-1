using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace TVAttendance.Models
{
    public class Singer
    {
        [Key]
        public int SingerID { get; set; }

        [MaxLength(50)]
        [Required]
        public string SingerFirstName { get; set; }

        [MaxLength(50)]
        [Required]
        public string SingerLastName { get; set; }

        public DateTime SingerDOB { get; set; }
        public string SingerAddress { get; set; }
        public bool SingerStatus { get; set; }
        public DateTime SingerRegisterDate { get; set; }

        public string SingerEmergContactFirstName { get; set; }
        public string SingerEmergContactLastName { get; set; }
        public string SingerEmergContactPhone { get; set; }

        // Foreign Key
        [Required]
        public int ChapterID { get; set; }
        public Chapter Chapter { get; set; }

        // Many-to-Many with Program
        public ICollection<SingerProgram> SingerPrograms { get; set; }
    }
}
