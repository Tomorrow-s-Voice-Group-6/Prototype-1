namespace TVAttendance.ViewModels
{
    public class CalendarVM
    {
        public DateTime Date { get; set; } //Don't want to seperate date by date and time since it's too much work,
        //Instead just use one DateTime to compare shiftstart and shiftend with the hour of the DateTime object
        public List<EventListVM> EventLst { get; set; } = new List<EventListVM>();
    }
}
