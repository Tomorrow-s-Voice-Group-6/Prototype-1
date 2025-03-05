using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace TVAttendance.Models
{
    public class Singer
    {
        public int ID { get; set; }

        [Display(Name = "First Name")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Required]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Required]
        public string LastName { get; set; }

        [Display(Name = "Date of Birth")]
        [Required]
        public DateTime DOB { get; set; }

        //Rue Sainte-Thérèse-de-l'Enfant-Jésus-du-Mont-Carmel is 51 characters excluding street numbers.
        //bumped up to 60 for good measure
        [StringLength(60)]
        [Required]
        public string Street {  get; set; }

        [StringLength(35)]
        [Display(Name = "City")]
        [Required]
        public string City { get; set; }

        [Required]
        public Province Province { get; set; }

        [StringLength(6)]
        [RegularExpression("^[ABCEGHJ-NPRSTVXY]\\d{1}[ABCEGHJ-NPRSTV-Z]\\d{1}[ABCEGHJ-NPRSTV-Z]\\d{1}", ErrorMessage = "Postal code is in an incorrect format")]
        [Display(Name = "Postal Code")]
        [Required]
        public string PostalCode { get; set; }

        [Display(Name = "Active")]
        public bool Status { get; set; }

        [Display(Name = "Registration Date")]
        [Required]
        public DateTime RegisterDate { get; set; }

        //thinking about removing emergency contact information for simplicity sake
        [Display(Name = "First Name")]
        [MaxLength(50, ErrorMessage = "Emergency contact first name cannot exceed 50 characters")]
        public string EmergencyContactFirstName { get; set; }

        [Display(Name = "Last Name")]
        [MaxLength(50, ErrorMessage = "Emergency contact last name cannot exceed 50 characters")]
        public string EmergencyContactLastName { get; set; }

        [Display(Name = "Phone Number")]
        [RegularExpression("^\\d{10}", ErrorMessage = "Phone number must be 10 digits in length")]
        [StringLength(10)]
        [Required]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string EmergencyContactPhone { get; set; }

        [Display(Name = "Chapter")]
        [Required]
        public int ChapterID { get; set; }
        public Chapter? Chapter { get; set; }

        public ICollection<SingerSession> SingerSessions { get; set; } = new HashSet<SingerSession>();

        #region Summary
        [Display(Name = "Name")]
        public string FullName => $"{FirstName} {LastName}";
        public string Summary => $"{FullName} - {DOB.ToShortDateString()}";

        public string DisplayDOB => DOB.ToShortDateString();

        public string DisplayRegistration => RegisterDate.ToShortDateString();

        [Display(Name = "Emergency Contact")]
        public string EmergFullName => $"{EmergencyContactFirstName} {EmergencyContactLastName}";

        public string DisplayPhone => $"{EmergencyContactPhone.Substring(0, 3)}-{EmergencyContactPhone.Substring(3, 3)}-{EmergencyContactPhone.Substring(6)}";

        [Display(Name = "Address")]
        public string Address => $"{this.Street}, {this.City}, {this.Province}, Canada, {this.PostalCode.Substring(0,3)} {this.PostalCode.Substring(3)}";
        #endregion

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.DOB.CompareTo(DateTime.Now) == 1)//if CompareTo() returns 1, then the DOB is later than now
            {
                yield return new ValidationResult("Singer date of birth cannot be in the future.", ["DOB"]);
            }

            if (this.RegisterDate > this.DOB)
            {
                yield return new ValidationResult("Singer cannot register in the future.", ["RegisterDate"]);
            }
        }
    }
}
