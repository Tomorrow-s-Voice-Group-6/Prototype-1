using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVAttendance.ViewModels
{
    public class SessionVM
    {

        [Display(Name = "Session Date")]
        public DateTime Date { get; set; }

        [NotMapped]
        public double AttendanceRate { get; set; }
    }
}
