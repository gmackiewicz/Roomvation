using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace Roomvation.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Participation> ReservationParticipants { get; set; }

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Participation>()
                .HasKey(p => new { p.ReservationId, p.ParticipantId });

            modelBuilder.Entity<Participation>()
                .HasRequired(p => p.Reservation)
                .WithMany()
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Participation>()
                .HasRequired(p => p.Participant)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Reservation>()
                .HasKey(r => r.Id)
                .HasRequired(r => r.Creator)
                .WithMany()
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}