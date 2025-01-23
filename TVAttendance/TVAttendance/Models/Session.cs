using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace TVAttendance.Models
{
    public class Session
    {
        public int ID { get; set; }

        [MaxLength(255, ErrorMessage = "Error, cannot have notes more than 255 characters")]
        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Date of Program")]
        public DateTime Date { get; set; }

        [Display(Name = "Chapter")]
        public int? ChapterID { get; set; }
        public Chapter? Chapter { get; set; }

        public ICollection<SingerSession> SingerSessions { get; set; } = new HashSet<SingerSession>();
        #region Summary
        public string? Summary => $"{Chapter?.City} - {Date}";
        #endregion
    }
}
