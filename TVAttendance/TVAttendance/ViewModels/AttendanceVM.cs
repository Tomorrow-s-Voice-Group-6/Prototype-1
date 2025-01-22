using TVAttendance.Models;

namespace TVAttendance.ViewModels
{
    public class AttendanceVM
    {
        /* View model to hold Session and a list of singers who attended said session */
        public Session Session { get; set; }
        public List<Singer> Singers { get; set;}
    }
}
