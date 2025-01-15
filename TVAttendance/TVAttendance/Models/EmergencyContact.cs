using System.ComponentModel.DataAnnotations;

namespace TVAttendance.Models
{
    public class EmergencyContact
    {
        public int ID { get; set; }
        [Display(Name = "Contact First Name")]
        [MaxLength(55, ErrorMessage = "Error: First name cannot be more than 50 characters")]
        public string FirstName { get; set; }
        [Display(Name = "Contact Last Name")]
        [MaxLength(100, ErrorMessage = "Error: Last name cannot be more than 100 characters")]
        [Required]
        public string LastName { get; set; }
        [Display(Name = "Contact Number")]
        [MaxLength(10, ErrorMessage = "Error: Phone number cannot be more than 10 digits")]
        [Required]
        //[RegularExpression()]
        public string Phone { get; set; }

        [Display(Name = "Notes")]
        [MaxLength(255)]
        public string Notes { get; set; }
        [Display(Name = "Relationship to person")]
        public Relationship Relationship { get; set; }
        public ICollection<Singer> Singers { get; set; } = new HashSet<Singer>();
        #region Summary
        public string FullName => $"{FirstName} {LastName}";
        public string Summary => $"{LastName}, {FirstName}\n{Phone} - {Relationship}";
        #endregion
    }
}
