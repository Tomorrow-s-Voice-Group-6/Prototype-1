namespace TVAttendance.ViewModels
{
    public class EventListVM
    {
        //Event ID and List of volunteer who attended the event
        public int ID { get; set; }
        public List<VolunteerVM> VolunteerLst { get; set; } = new List<VolunteerVM>();
    }
}
