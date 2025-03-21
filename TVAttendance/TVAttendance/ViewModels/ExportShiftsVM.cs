namespace TVAttendance.ViewModels
{
    public class ExportShiftsVM
    {
        public string Name { get; set; }
        public string Attended { get; set; }
        public TimeOnly startTime { get; set; }
        public TimeOnly endTime { get; set; }
        public string TimeWorked => (startTime - endTime).TotalHours.ToString();
        public string ShiftRange { get; set; }
        public string Notes { get; set; }
    }
}
