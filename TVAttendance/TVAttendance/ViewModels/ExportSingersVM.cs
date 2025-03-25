namespace TVAttendance.ViewModels
{
    public class ExportSingersVM
    {
        public string name { get; set; }
        public string DOB { get; set; }
        public string RegDate { get; set; }
        public string chapter { get; set; }
        public string emergencyName { get ; set; }  
        public string emergencyPhone { get; set; }

        //simple formatting string for phone number to display
        public string phoneFormatted => 
            $"({emergencyPhone.Substring(0, 3)})-{emergencyPhone.Substring(3, 3)}-{emergencyPhone.Substring(6, 4)}";


    }
}
