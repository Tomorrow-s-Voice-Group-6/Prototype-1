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
        public DbSet<EmergencyContact> EmergencyContacts { get; set; }

        //Fluent API
    }
}
