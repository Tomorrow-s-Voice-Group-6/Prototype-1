using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TVAttendance.Models;
using TVAttendance.Data;
using System.Diagnostics;

namespace TVAttendance.Data
{
    public class TVInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider,
            bool DeleteDatabase = false, bool UseMigrations = true, bool SeedSampleData = true)
        {
            using (var context = new TomorrowsVoiceContext(
                serviceProvider.GetRequiredService<DbContextOptions<TomorrowsVoiceContext>>()))
            {
                #region Prepare the Database
                try
                {
                    if (DeleteDatabase || !context.Database.CanConnect())
                    {
                        context.Database.EnsureDeleted();
                        if (UseMigrations)
                        {
                            context.Database.Migrate();
                        }
                        else
                        {
                            context.Database.EnsureCreated();
                        }
                    }
                    else
                    {
                        if (UseMigrations)
                        {
                            context.Database.Migrate();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.GetBaseException().Message);
                }
                #endregion

                // If Cities exist abort, seeddata likely already made
                if (context.Cities.Any())
                {
                    return;
                }

                var random = new Random();

                var firstNames = new List<string>
                {
                    "Olivia", "Emma", "Ava", "Sophia", "Isabella", "Mia", "Charlotte", "Amelia", "Harper", "Evelyn",
                    "Liam", "Noah", "Oliver", "Elijah", "James", "William", "Benjamin", "Lucas", "Henry", "Alexander",
                    "Ella", "Scarlett", "Grace", "Chloe", "Victoria", "Avery", "Hannah", "Addison", "Aria", "Ellie",
                    "Jack", "Logan", "Sebastian", "Mason", "Ethan", "Matthew", "Joseph", "Daniel", "David", "Carter",
                    "Levi", "Wyatt", "Gabriel", "Julian", "Isaiah", "Lincoln", "Anthony", "Andrew", "Hudson", "Christopher"
                };

                var lastNames = new List<string>
                {
                    "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez",
                    "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin",
                    "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson",
                    "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores"
                };
                var addresses = new List<string> { "Main St", "Broadway", "Elm St", "Maple Ave", "Oak St", "Pine Ln", "Cedar Rd", "Spruce Blvd", "Willow Dr", "Birch Ct" };

                // Seed Cities
                var cities = new List<City>
                {
                    new City { CityName = "New York" },
                    new City { CityName = "Los Angeles" },
                    new City { CityName = "Chicago" }
                };
                context.Cities.AddRange(cities);
                context.SaveChanges();

                // Seed Directors
                var directors = cities.Select(city => new Director
                {
                    FirstName = firstNames[random.Next(firstNames.Count)],
                    LastName = lastNames[random.Next(lastNames.Count)],
                    DOB = new DateTime(1970 + random.Next(20), random.Next(1, 13), random.Next(1, 28)),
                    HireDate = DateTime.Now.AddYears(-random.Next(1, 10)),
                    Address = $"{random.Next(100, 999)} {addresses[random.Next(addresses.Count)]}, {city.CityName}",
                    Email = $"{city.CityName.ToLower()}_director{random.Next(1000)}@example.com",
                    Phone = $"{random.Next(100, 999)}-{random.Next(100, 999)}-{random.Next(1000, 9999)}",
                    Status = random.Next(0, 2) == 1
                }).ToList();
                context.Directors.AddRange(directors);
                context.SaveChanges();
                directors = context.Directors.ToList();
                // Seed Chapters
                var chapters = cities.Select(city => new Chapter
                {
                    City = city.CityName,
                    Address = $"{random.Next(100, 999)} {addresses[random.Next(addresses.Count)]}, {city.CityName}",
                    ID = city.CityID,
                    DirectorID = directors[random.Next(directors.Count)].ID,
                }).ToList();
                context.Chapters.AddRange(chapters);
                context.SaveChanges();

                // Seed Singers
                var singers = new List<Singer>();

                foreach (var chapter in chapters)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var LastName = lastNames[random.Next(lastNames.Count)];
                        singers.Add(new Singer
                        {
                            FirstName = firstNames[random.Next(firstNames.Count)],
                            LastName = LastName,
                            DOB = new DateTime(1990 + random.Next(10), random.Next(1, 13), random.Next(1, 28)),
                            RegisterDate = DateTime.Now.AddMonths(-random.Next(1, 60)),
                            Address = $"{random.Next(100, 999)} {addresses[random.Next(addresses.Count)]}, {chapter.City}",
                            Status = random.Next(0, 2) == 1,
                            EmergencyContactFirstName = firstNames[random.Next(firstNames.Count)],
                            EmergencyContactLastName = LastName,
                            EmergencyContactPhone = $"555-{random.Next(100, 999)}-{random.Next(1000, 9999)}",
                            ChapterID = chapter.ID
                        });
                    }
                }

                context.Singers.AddRange(singers);
                context.SaveChanges();

                // Seed Sessions
                var sessions = cities.Select(city => new Session
                {
                    Notes = $"Description for {city.CityName} program",
                    Date = DateTime.Now.AddDays(-random.Next(30, 365)),
                    ID = city.CityID,
                    ChapterID = city.CityID
                }).ToList();
                context.Sessions.AddRange(sessions);
                context.SaveChanges();

                // Seed SingerSessions
                var singerSessions = new List<SingerSession>();
                foreach (var session in sessions)
                {
                    var citySingers = singers.Where(s => s.ChapterID == session.ID).Take(5).ToList();
                    foreach (var singer in citySingers)
                    {
                        singerSessions.Add(new SingerSession
                        {
                            SingerID = singer.ID,
                            SessionID = session.ID,
                            Notes = $"Attendance record for {singer.FirstName} {singer.LastName} in {session.Notes}"
                        });
                    }
                }
                context.SingerSessions.AddRange(singerSessions);
                context.SaveChanges();
            }
        }
    }
}