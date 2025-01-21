using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVAttendance.Models
{
    public class SingerSession
    {
        //No ID because its a join table
        [Display(Name = "Singer")]
        public int SingerID { get; set; }
        public Singer? Singer { get; set; }

        [Display(Name = "Program")]
        public int ProgramID { get; set; }
        public Session? Program { get; set; }
        [Display(Name = "Notes")]
        [MaxLength(255, ErrorMessage = "Error: Cannot have notes greater than 255 characters")]
        public string? Notes { get; set; }
    }
}
