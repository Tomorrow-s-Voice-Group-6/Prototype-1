using System.ComponentModel.DataAnnotations;

namespace TVAttendance.ViewModels
{
    public class UsersVM
    {
        [Display(Name="User")]
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
