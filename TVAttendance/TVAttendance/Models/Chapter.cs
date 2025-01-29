using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace TVAttendance.Models
{
    public class Chapter
    {
        public int ID { get; set; }

        [MaxLength(100, ErrorMessage = "City name cannot exceed 100 characters.")]
        [Required(ErrorMessage = "City name cannot be empty.")]
        [Display(Name = "City")]
        public string City { get; set; }

        [MaxLength(255)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Director is required.")]
        [Display(Name = "Choir director")]
        public int DirectorID { get; set; }
        public Director? Director { get; set; }

        public ICollection<Session> Sessions { get; set; } = new HashSet<Session>();
        public ICollection<Volunteer> Volunteers { get; set; } = new HashSet<Volunteer>();
        public ICollection<Singer> Singers { get; set; } = new HashSet<Singer>();
    }
}
