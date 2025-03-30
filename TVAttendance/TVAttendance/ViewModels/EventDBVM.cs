using TVAttendance.Models;

namespace TVAttendance.ViewModels
{
    public class EventDBVM
    {
        public List<Volunteer> Volunteers { get; set; }

        public List<ShiftVolunteer> Shifts { get; set; }

        public List<Event> Events { get; set; }
    }
}
