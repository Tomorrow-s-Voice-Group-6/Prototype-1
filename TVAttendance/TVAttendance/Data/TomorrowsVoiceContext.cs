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
        public DbSet<Event> Events { get; set; }
        public DbSet<VolunteerEvent> VolunteerEvents { get; set; }

        //Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Composite Keys
            modelBuilder.Entity<SingerSession>()
                .HasKey(sp => new { sp.SingerID, sp.SessionID });

            modelBuilder.Entity<VolunteerEvent>()
                .HasKey(ve=>new {ve.EventID, ve.VolunteerID });

            //Unique Constraints
            modelBuilder.Entity<Director>()
                .HasIndex(d => new { d.FirstName, d.LastName, d.DOB })
                .IsUnique();

            modelBuilder.Entity<Singer>()
                .HasIndex(s => new { s.FirstName, s.LastName, s.DOB })
                .IsUnique();

            modelBuilder.Entity<Event>()
                .HasIndex(e => new { e.EventName, e.EventStreet })
                .IsUnique();

            modelBuilder.Entity<Volunteer>()
                .HasIndex(e => new {e.FirstName, e.LastName, e.DOB })
                .IsUnique();

            //m:m relationship
            modelBuilder.Entity<Chapter>()
                .HasMany<Session>(p=>p.Sessions)
                .WithOne(c=>c.Chapter)
                .HasForeignKey(c=>c.ChapterID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Chapter>()
                .HasMany<Director>(p => p.Directors)
                .WithMany(c => c.Chapters);

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

            //Cascade delete is obsolete.  No delete action that the user can perform.
            modelBuilder.Entity<Event>()
                .HasMany<VolunteerEvent>(ve => ve.VolunteerEvents)
                .WithOne(e => e.Event)
                .HasForeignKey(e => e.EventID);
            modelBuilder.Entity<Volunteer>()
                .HasMany<VolunteerEvent>(ve => ve.VolunteerEvents)
                .WithOne(e => e.Volunteer)
                .HasForeignKey(e => e.VolunteerID);
        }
    }
}
