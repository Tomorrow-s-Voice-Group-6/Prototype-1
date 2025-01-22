using Microsoft.EntityFrameworkCore;
using TVAttendance.Models;

namespace TVAttendance.Data
{
    public class TomorrowsVoiceContext : DbContext
    {
        public TomorrowsVoiceContext(DbContextOptions<TomorrowsVoiceContext> options) 
            : base(options) 
        {

        }

        public DbSet<Singer> Singers { get; set; }
        public DbSet<Director> Directors { get; set; }
        public DbSet<Volunteer> Volunteers { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<SingerSession> SingerSessions { get; set; }
        public DbSet<City> Cities { get; set; }

        //Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Composite Key for SingerProgram table
            modelBuilder.Entity<SingerSession>()
                .HasKey(sp => new { sp.SingerID, sp.SessionID });

            //Unique Constraints - Director, Singer
            modelBuilder.Entity<Director>()
                .HasIndex(d => new { d.FirstName, d.LastName, d.DOB })
                .IsUnique();

            modelBuilder.Entity<Singer>()
                .HasIndex(s => new { s.FirstName, s.LastName, s.DOB })
                .IsUnique();

            //m:m relationship
            modelBuilder.Entity<Chapter>()
                .HasMany<Session>(p=>p.Sessions)
                .WithOne(c=>c.Chapter)
                .HasForeignKey(c=>c.ChapterID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Singer>()
                .HasMany<SingerSession>(sp=>sp.SingerSessions)
                .WithOne(s=>s.Singer)
                .HasForeignKey(s=>s.SingerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Session>()
                .HasMany<SingerSession>(sp=>sp.SingerSessions)
                .WithOne(p=>p.Session)
                .HasForeignKey(p=>p.SessionID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
