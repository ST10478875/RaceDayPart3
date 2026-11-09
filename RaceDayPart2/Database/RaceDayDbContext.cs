using Microsoft.EntityFrameworkCore;
using RaceDay.API.models;
using System.Collections.Generic;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        // Constructor accepting dependency injection for database options (e.g., SQL Server connection)
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

        // DbSet tables mapped to database relational tables
        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Enrolment> Enrolments => Set<Enrolment>();
        public DbSet<Result> Results => Set<Result>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure precision for decimal properties to prevent truncation warnings
            modelBuilder.Entity<Event>()
                .Property(e => e.Distance)
                .HasPrecision(18, 2);

            // Configure foreign key relationship and restrict cascading deletes for organiser events
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(u => u.Events)
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Explicitly mapped collection navigation properties and restrict deletes to prevent cascading conflicts
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.User)
                .WithMany(u => u.Enrolments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Event)
                .WithMany(ev => ev.Enrolments)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
