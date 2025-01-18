using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace TVAttendance.Models
{
    public class Program //Potential naming issue, double check you're grabbing the write file (Program.cs)
    {
        public int ID { get; set; }

        [MaxLength(255, ErrorMessage = "Error, cannot have notes more than 255 characters")]
        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Date of Program")]
        public DateTime Date { get; set; }

        [Display(Name = "Chapter")]
        public int? ChapterID { get; set; }
        public Chapter? Chapter { get; set; }

       public ICollection<SingerProgram> SingerPrograms { get; set; } = new HashSet<SingerProgram>();
    }
}
