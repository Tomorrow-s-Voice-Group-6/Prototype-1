using TVAttendance.Models;

namespace TVAttendance.ViewModels
{
    public class ExportFilterVM
    {
        public string chapter { get; set; }

        public string startdate { get; set; }
        public string enddate { get; set; }
        public int attended { get; set; }
        public List<string> directors { get; set; }
    }
}
