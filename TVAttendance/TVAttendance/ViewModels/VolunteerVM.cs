namespace TVAttendance.ViewModels
{
    public class VolunteerVM
    {
        /* Volunteer View Model contains the ID for database management, name for user experience,
         and we don't need shift attended, because the volunteer shoulnd't be in the Event VM
        if they didn't attend. */
        //Carter W
        public int? ID { get; set; }
        public string Name { get; set; }
        public DateTime ShiftStart { get; set; }
        public DateTime ShiftEnd { get; set; }
    }
}
