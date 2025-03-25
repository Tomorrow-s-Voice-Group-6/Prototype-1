namespace TVAttendance.ViewModels
{
    public class ExportShiftsVM
    {
        public string Name { get; set; }
        public string Attended { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public string TimeWorked => $"{(endTime.Hour - startTime.Hour):F2} hours";
        public string ShiftRange { get; set; }
        public string Notes { get; set; }
    }
}
