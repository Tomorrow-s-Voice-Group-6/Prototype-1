﻿namespace TVAttendance.Models
{
    public class ShiftVolunteer
    {
        public int ShiftID { get; set; }
        public Shift? Shift { get; set; }
        public int VolunteerID { get; set; }
        public Volunteer? Volunteer { get; set; }
        public bool ShiftAttended { get; set; }
        public string? NonAttendanceNote { get; set; }
    }
}
