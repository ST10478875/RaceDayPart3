
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.models;

namespace RaceDay.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(RaceDayDbContext context)
        {
            // Ensure that the database and its tables exist.
            context.Database.EnsureCreated();

            // Create a password hasher to securely store sample passwords.
            var passwordHasher = new PasswordHasher<User>();

            // Temporary password used for the sample testing accounts.
            const string demoPassword = "RaceDay@123";

            // Check whether users already exist in the database.
            var existingUsers = context.Users.ToList();

            if (existingUsers.Any())
            {
                // Fix the old sample password hashes without changing
                // passwords belonging to other existing users.
                foreach (var user in existingUsers)
                {
                    if (user.PasswordHash == "hashed_password_123")
                    {
                        user.PasswordHash = passwordHasher.HashPassword(
                            user, demoPassword);
                    }
                }

                // Save any updated sample passwords.
                context.SaveChanges();

                // Do not create duplicate sample accounts or events.
                return;
            }

            // Create the sample Organiser account.
            var organiser = new User
            {
                Username = "LwazzyCoach",
                Email = "organiser@raceday.co.za",
                Role = "Organizer"
            };

            // Hash the Organiser's password before saving.
            organiser.PasswordHash = passwordHasher.HashPassword(
                organiser, demoPassword);

            // Create the sample Participant account.
            var participant = new User
            {
                Username = "SowetoRunner",
                Email = "runner@raceday.co.za",
                Role = "Participant"
            };

            // Hash the Participant's password before saving.
            participant.PasswordHash = passwordHasher.HashPassword(
                participant, demoPassword);

            // Add both sample accounts to the database.
            context.Users.AddRange(organiser, participant);
            context.SaveChanges();

            // Create a sample running event managed by the Organiser.
            var raceEvent = new Event
            {
                Name = "Soweto Spring Marathon",
                Description = "Annual 10km road running and walking event through Moroka and Dlamini",
                Date = DateTime.UtcNow.AddDays(30),
                Location = "Vilakazi Street, Soweto",
                Distance = 10.0m,
                EventType = "Run",
                OrganiserId = organiser.Id
            };

            // Save the event to generate its database ID.
            context.Events.Add(raceEvent);
            context.SaveChanges();

            // Create a category for the sample event.
            var category = new Category
            {
                Name = "Senior Open (10km)",
                EventId = raceEvent.Id
            };

            // Save the category.
            context.Categories.Add(category);
            context.SaveChanges();

            // Create a sample enrolment for the Participant.
            var enrolment = new Enrolment
            {
                UserId = participant.Id,
                EventId = raceEvent.Id,
                CategoryId = category.Id,
                EnrolmentDate = DateTime.UtcNow
            };

            // Save the sample enrolment.
            context.Enrolments.Add(enrolment);
            context.SaveChanges();
        }
    }
}
