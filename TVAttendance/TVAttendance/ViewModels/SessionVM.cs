using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TVAttendance.Models;

namespace TVAttendance.ViewModels
{
    public class SessionVM
    {
        public int ID { get; set; }

        public Chapter? Chapter { get; set; }
        public List<Chapter> Chapters { get; set; }

        [Display(Name = "Session Date")]
        public DateTime Date { get; set; }

        [NotMapped]
        public double AttendanceRate { get; set; }
    }
}
