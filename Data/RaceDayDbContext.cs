using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

namespace RaceDayAPI.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<OrganiserProfile> OrganiserProfiles { get; set; }
        public DbSet<ParticipantProfile> ParticipantProfiles { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<EventEnrolment> EventEnrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<OrganiserProfile>()
                .HasIndex(op => op.UserID)
                .IsUnique();

            modelBuilder.Entity<ParticipantProfile>()
                .HasIndex(pp => pp.UserID)
                .IsUnique();

            modelBuilder.Entity<EventEnrolment>()
                .HasIndex(ee => new { ee.CategoryID, ee.ParticipantID })
                .IsUnique();

            modelBuilder.Entity<EventEnrolment>()
                .HasIndex(ee => ee.BibNumber)
                .IsUnique()
                .HasFilter("[BibNumber] IS NOT NULL");

            modelBuilder.Entity<Result>()
                .HasIndex(r => r.EnrolmentID)
                .IsUnique();

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany()
                .HasForeignKey(e => e.OrganiserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event)
                .WithMany()
                .HasForeignKey(c => c.EventID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventEnrolment>()
                .HasOne(ee => ee.Category)
                .WithMany()
                .HasForeignKey(ee => ee.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventEnrolment>()
                .HasOne(ee => ee.Participant)
                .WithMany()
                .HasForeignKey(ee => ee.ParticipantID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Result>()
                .HasOne(r => r.Enrolment)
                .WithMany()
                .HasForeignKey(r => r.EnrolmentID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}