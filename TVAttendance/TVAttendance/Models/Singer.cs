namespace TVAttendance.Models
{
    public class Singer
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool Status { get; set; }
        public DateOnly RegisterDate { get; set; }
        public string EmergencyContactFirstName { get; set; }
        public string EmergencyContactLastName { get; set; }
        public string EmergencyContactPhone { get; set; }

        /*public Chapter ChapterID { get; set; } *///Foreign key
    }
}
