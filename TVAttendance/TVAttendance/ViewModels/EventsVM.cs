namespace TVAttendance.ViewModels
{
    public class EventsVM
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Location => $"{City}, {Province.ToUpper()}";
    }
}
