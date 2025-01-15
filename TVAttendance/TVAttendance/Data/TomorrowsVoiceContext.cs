using Microsoft.EntityFrameworkCore;
using TVAttendance.Models;

namespace TVAttendance.Data
{
    public class TomorrowsVoiceContext : DbContext
    {
        public TomorrowsVoiceContext(DbContextOptions<TomorrowsVoiceContext> options) : base(options) { }

        public DbSet<Singer> Singers { get; set; }
        //Fluent API
    }
}
