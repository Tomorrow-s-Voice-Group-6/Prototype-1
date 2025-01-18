using Microsoft.EntityFrameworkCore;
using TVAttendance.Models;

namespace TVAttendance.Data
{
    public class TomorrowsVoiceContext : DbContext
    {
        public TomorrowsVoiceContext(DbContextOptions<TomorrowsVoiceContext> options) : base(options) { }

        public DbSet<Singer> Singers { get; set; }
        public DbSet<Director> Directors { get; set; }
        public DbSet<Volunteer> Volunteers { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Models.Program> Programs { get; set; } //Naming convention might be an issue
        public DbSet<SingerProgram> SingerPrograms { get; set; }


        //Fluent API
    }
}
