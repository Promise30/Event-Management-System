using Event_Management_System.API.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Event_Management_System.API.Infrastructures
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<EventCentre> EventCentres { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<EventCentreAvailability> Availabilities { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;
        public DbSet<TicketType> TicketTypes { get; set; } = null!;
       // public DbSet<Payment> Payments { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // EventCentre & Booking (1:M)
            builder.Entity<EventCentre>()
                .HasMany(ec => ec.Bookings)
                .WithOne(b => b.EventCentre)
                .HasForeignKey(b => b.EventCentreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Event & Ticket (1:M)
            builder.Entity<Event>()
                .HasMany(e => e.TicketTypes)
                .WithOne(e => e.Event)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Booking & Event (1:1)
            builder.Entity<Booking>()
                .HasOne(e => e.Event)
                .WithOne(b => b.Booking)
                .HasForeignKey<Event>(e => e.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Booking & Organizer
            builder.Entity<Booking>()
                .HasOne(e => e.Organizer)
                .WithMany()
                .HasForeignKey(e =>e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket & Attendee (1:M)
            builder.Entity<Ticket>()
                .HasOne(t => t.Attendee)
                .WithMany()
                .HasForeignKey(t => t.AttendeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // TicketType & Ticket (1:M)
            builder.Entity<TicketType>()
                .HasMany(tt => tt.Tickets)
                .WithOne(t => t.TicketType)
                .HasForeignKey(t => t.TicketTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<TicketType>()
                .Property(tt => tt.Price)
                .HasPrecision(18, 2);

            //// Payment relationships
            //builder.Entity<Payment>()
            //    .HasOne(p => p.User)
            //    .WithMany()
            //    .HasForeignKey(p => p.UserId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //builder.Entity<Payment>()
            //    .Property(p => p.Amount)
            //    .HasPrecision(18, 2);

            //builder.Entity<Payment>()
            //    .HasIndex(p => p.TransactionReference)
            //    .IsUnique();


            var adminRole = new IdentityRole<Guid>
            {
                Id = Guid.Parse("4f9d64fb-074c-47cd-9d1a-4448a299dbe7"),
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR",
                ConcurrencyStamp = "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
            };
            var organizerRole = new IdentityRole<Guid>
            {
                Id = Guid.Parse("9d8be1e7-c6f5-4fd2-aa02-8122aee41da6"),
                Name = "Organizer",
                NormalizedName = "ORGANIZER",
                ConcurrencyStamp = "b2c3d4e5-f6a7-8901-bcde-f12345678901"
            };
            var userRole = new IdentityRole<Guid>
            {
                Id = Guid.Parse("b856684b-965b-47f8-84fe-ebc342f1aec8"),
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = "c3d4e5f6-a7b8-9012-cdef-123456789012"
            };

            builder.Entity<IdentityRole<Guid>>()
                .HasData(adminRole, organizerRole, userRole);

            // seed an admin user, organizer user and a normal user
            var hasher = new PasswordHasher<ApplicationUser>();
            var fixedDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);

            var adminUser = new ApplicationUser
            {
                Id = Guid.Parse("cb7c65a0-54ef-40f2-9b87-4f63d1a9da19"),
                UserName = "admin@gmail.com",
                NormalizedUserName = "ADMIN@GMAIL.COM",
                Email = "admin@gmail.com",
                NormalizedEmail = "ADMIN@GMAIL.COM",
                EmailConfirmed = true,
                SecurityStamp = "d4e5f6a7-b8c9-0123-def1-234567890123",
                ConcurrencyStamp = "6dcb4438-a7e7-401a-bd98-8e6a494d0701",
                PasswordHash = "AQAAAAIAAYagAAAAEIv/wuUfB3rVJhNJwYgEigFe32QmZupPpjW46tonsf8vpdzMPYfBFOklTRdHiSFASg==",
                FirstName = "System",
                LastName = "Admin",
                PhoneNumber = "1234567890",
                PhoneNumberConfirmed = true,
                CreatedDate = fixedDate,
                ModifiedDate = fixedDate
            };
            var organizerUser = new ApplicationUser
            {
                Id = Guid.Parse("53765d3f-99f2-4932-9ab6-ee6b1940fd76"),
                UserName = "organizer@gmail.com",
                NormalizedUserName = "ORGANIZER@GMAIL.COM",
                Email = "organizer@gmail.com",
                NormalizedEmail = "ORGANIZER@GMAIL.COM",
                EmailConfirmed = true,
                SecurityStamp = "e5f6a7b8-c9d0-1234-ef12-345678901234",
                ConcurrencyStamp = "92271f9d-c3b8-4130-a841-4aec28fa0bac",
                PasswordHash = "AQAAAAIAAYagAAAAEANzpRDRLOWZRPT1ZhW8LK3AccswX5naRbdxQQ6TpVA7GyK+YKK+GsC4tqpH48oMYg==",
                FirstName = "Event",
                LastName = "Organizer",
                PhoneNumber = "0987654321",
                PhoneNumberConfirmed = true,
                CreatedDate = fixedDate,
                ModifiedDate = fixedDate
            };
            var normalUser = new ApplicationUser
            {
                Id = Guid.Parse("61310700-9580-4e8e-b472-e3f04b12c8d8"),
                UserName = "user@gmail.com",
                NormalizedUserName = "USER@GMAIL.COM",
                Email = "user@gmail.com",
                NormalizedEmail = "USER@GMAIL.COM",
                EmailConfirmed = true,
                SecurityStamp = "f6a7b8c9-d0e1-2345-f123-456789012345",
                ConcurrencyStamp = "dc8f584a-2976-4005-b6ac-0f0f54c09a91",
                PasswordHash = "AQAAAAIAAYagAAAAEP8tWgzIJapX6iyHZxLlrIOg4TTkKT4X/t5jAfA0uTnlk9iZBgZqmMfCU/yKR6LRTA==",
                FirstName = "Normal",
                LastName = "User",
                PhoneNumber = "1234567890",
                PhoneNumberConfirmed = true,
                CreatedDate = fixedDate,
                ModifiedDate = fixedDate
            };

            // Seed the users
            builder.Entity<ApplicationUser>().HasData(adminUser, organizerUser, normalUser);
            // Assign roles to the users and roles
            builder.Entity<IdentityUserRole<Guid>>().HasData(
                new IdentityUserRole<Guid> { UserId = adminUser.Id, RoleId = adminRole.Id }, // Admin
                new IdentityUserRole<Guid> { UserId = organizerUser.Id, RoleId = organizerRole.Id}, // Organizer
                new IdentityUserRole<Guid> { UserId = normalUser.Id, RoleId = userRole.Id }  // Normal User
            );
        }
    }
}
