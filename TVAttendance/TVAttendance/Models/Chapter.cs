using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace TVAttendance.Models
{
    public class Chapter
    {
        public int ID { get; set; }

        [MaxLength(100)]
        [Required]
        [Display(Name = "City")]
        public string City { get; set; }

        [MaxLength(255)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Choir director")]
        public int? DirectorID { get; set; }
        public Director? Director { get; set; }

        [Display(Name = "Program")]
        public int? ProgramID { get; set; }
        public Program? Program { get; set; }

        //Confused about collection of programs - does a chapter have 1 program or multiple?
        //public ICollection<Program> Programs { get; set; } = new HashSet<Program>();
        public ICollection<Volunteer> Volunteers { get; set; } = new HashSet<Volunteer>();
        public ICollection<Singer> Singers { get; set; } = new HashSet<Singer>();
    }
}
