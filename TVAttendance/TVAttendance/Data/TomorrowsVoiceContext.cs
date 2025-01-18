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
        public DbSet<Models.Program> Programs { get; set; } //Naming convention might be an issue
        public DbSet<SingerProgram> SingerPrograms { get; set; }

        //Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Composite Key for SingerProgram table
            modelBuilder.Entity<SingerProgram>()
                .HasKey(sp => new { sp.SingerID, sp.ProgramID });

            //Unique Constraints - Director, Singer
            modelBuilder.Entity<Director>()
                .HasIndex(d => new { d.FirstName, d.LastName, d.DOB })
                .IsUnique();

            modelBuilder.Entity<Singer>()
                .HasIndex(s => new { s.FirstName, s.LastName, s.DOB })
                .IsUnique();

            //m:m relationship
            modelBuilder.Entity<Chapter>()
                .HasMany<Models.Program>(p=>p.Programs)
                .WithOne(c=>c.Chapter)
                .HasForeignKey(c=>c.ChapterID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Singer>()
                .HasMany<SingerProgram>(sp=>sp.SingerPrograms)
                .WithOne(s=>s.Singer)
                .HasForeignKey(s=>s.SingerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Models.Program>()
                .HasMany<SingerProgram>(sp=>sp.SingerPrograms)
                .WithOne(p=>p.Program)
                .HasForeignKey(p=>p.ProgramID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
