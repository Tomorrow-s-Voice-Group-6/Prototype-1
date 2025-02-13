using System.ComponentModel.DataAnnotations;
using TVAttendance.Models;

namespace TVAttendance.Models
{
    public class Chapter
    {
        public int ID { get; set; }

        // Expanding address section
        [MaxLength(255)]
        [Required]
        [Display(Name = "Street Address")]
        public string Street { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required]
        [Display(Name = "Province")]
        public Province Province { get; set; }

        [StringLength(6)]
        [RegularExpression(@"^[ABCEGHJ-NPRSTVXY]\d{1}[ABCEGHJ-NPRSTV-Z]\d{1}[ABCEGHJ-NPRSTV-Z]\d{1}$",
        ErrorMessage = "Postal code is in an incorrect format")]
        [Display(Name = "Postal Code")]
        public string? ZipCode { get; set; } 


        // Director Info
        [Required(ErrorMessage = "Director is required.")]
        [Display(Name = "Choir Director")]
        public int DirectorID { get; set; }
        public Director? Director { get; set; }

        // 📌 **Chapter Relationships**
        public ICollection<Session> Sessions { get; set; } = new HashSet<Session>();
        public ICollection<Volunteer> Volunteers { get; set; } = new HashSet<Volunteer>();
        public ICollection<Singer> Singers { get; set; } = new HashSet<Singer>();

        // 📌 **Status Property for Archiving Chapters**
        [Required]
        [Display(Name = "Chapter Status")]
        public ChapterStatus Status { get; set; } = ChapterStatus.Active;
    }

    // 📌 **Enum for Chapter Status (Active or Archived)**
    public enum ChapterStatus
    {
        Active,
        Archived
    }
}
