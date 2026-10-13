using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RaceDay_2.Models;

using System.Data.SqlClient;

namespace RaceDay_2.Models
{
    //start of namespace

    // code first de context for raceday 
    // should be simialr to table names and constratints in the sql script

    public class RaceDay_Context : DbContext
    {
        //start of class
        public RaceDay_Context(DbContextOptions<RaceDay_Context> options) : base(options) { }

        //dbs a collection of entities mapped to a database table

        public DbSet<Roles> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }

        //
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //start of model configuration
            base.OnModelCreating(modelBuilder);


            //start of role configuration
            modelBuilder.Entity<Roles>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasIndex(r => r.RoleName).IsUnique();
            });
            //end of role configuration

            //start of user configuraction
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasIndex(u => u.Email).IsUnique();

                entity.HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleID);
            });
            //end of user configuration

            //start of category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");
                entity.HasCheckConstraint("ck_category_distancekm", "DistanceKM > 0");
                entity.HasCheckConstraint("ck_category_maxparticipants", "MaxParticipants > 0");

                entity.Property(c => c.DistanceKM).HasColumnType("decimal(5,2)");

                entity.HasOne(c => c.Event).WithMany(e => e.Categories).HasForeignKey(c => c.EventID);
            });
            //end of category configuration

            //start of enrolment Configuration
            modelBuilder.Entity<Enrolment>(entity =>
            {
                entity.ToTable("Enrolment", table =>table.HasCheckConstraint("ck_enrolment_status","Status IN ('Confirmed', 'Cancelled')"));

                //blocks the same person from siging up for the same category twice
                entity.HasIndex(en => new { en.ParticipantID, en.CategoryID }).IsUnique();

                entity.HasOne(en => en.Participant).WithMany().HasForeignKey(en => en.ParticipantID);

                entity.HasOne(en => en.Category).WithMany(c => c.Enrolments).HasForeignKey(en => en.CategoryID);

            });
            //3nd of enrolment

            //start of Result Configuration
            modelBuilder.Entity<Result>(entity =>
            {
                entity.ToTable("Result", table => table.HasCheckConstraint("ck_result_position","Position > 0"));

                //one result per enrolmentt
                entity.HasIndex(r => r.EnrolmentID).IsUnique();
                entity.HasOne(r => r.Enrolment).WithOne(en => en.Result).HasForeignKey<Result>(r => r.EnrolmentID);
                entity.HasOne(r => r.CapturedBy);
            });
            //end of result configuration

            //start of event configuration
            modelBuilder.Entity<Event>(entity =>
            {

                entity.ToTable("Event", table => table.HasCheckConstraint("ck_event_eventtype", "EventType IN ('Running', 'Walking', 'Cycling'"));
            });
        

        //end of model configuration

    }
        //end of class
    }

    //end of namespace
}
