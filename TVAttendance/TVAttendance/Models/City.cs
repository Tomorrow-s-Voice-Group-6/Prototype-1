using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TVAttendance.Models
{
    /* Not required so don't use this table just didn't want to delete it yet */
    public class City
    {
        [Key]
        public int CityID { get; set; } // Primary Key

        [Display(Name = "City Name")]
        [MaxLength(100)]
        [Required]
        public string CityName { get; set; }

        // Navigation Properties
        public ICollection<Program> Programs { get; set; } // One-to-Many with Program
        public ICollection<Chapter> Chapters { get; set; } // One-to-Many with Chapter
        public ICollection<Singer> Singers { get; set; } // One-to-Many with Singer
    }
}
