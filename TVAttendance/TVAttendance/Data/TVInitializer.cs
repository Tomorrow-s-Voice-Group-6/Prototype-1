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

                if (context.Singers.Any())
                {
                    return;
                }
                #endregion

                if (context.Singers.Select(s => s.ID).Count() > 0) ;

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
                var cities = new List<string>
                {
                    "Saskatoon",
                    "St Catharines",
                    "Hamilton",
                    "Toronto",
                    "Surrey",
                    "Vancouver"
                };

                var directors = new List<Director>();

                // Seed Directors
                foreach (var city in cities)
                {
                    directors.Add(new Director
                    {
                        FirstName = firstNames[random.Next(firstNames.Count)],
                        LastName = lastNames[random.Next(lastNames.Count)],
                        DOB = new DateTime(1970 + random.Next(20), random.Next(1, 13), random.Next(1, 28)),
                        HireDate = DateTime.Now.AddYears(-random.Next(1, 10)),
                        Address = $"{random.Next(100, 999)} {addresses[random.Next(addresses.Count)]}, {city}",
                        Email = $"{city}_director{random.Next(1000)}@example.com",
                        Phone = $"{random.Next(100, 999)}-{random.Next(100, 999)}-{random.Next(1000, 9999)}",
                        Status = true
                    });
                }

                context.Directors.AddRange(directors);
                context.SaveChanges();

                // Seed Chapters
                var chapters = new List<Chapter>();
                int dirCount = 0;

                foreach (string city in cities)
                {
                    chapters.Add(new Chapter
                    {
                        City = city,
                        Address = $"{random.Next(100, 999)} {addresses[random.Next(addresses.Count)]}, {city}",
                        DirectorID = directors[dirCount].ID
                    });

                    dirCount++;
                }
                context.Chapters.AddRange(chapters);
                context.SaveChanges();

                // Seed Singers
                var singers = new List<Singer>();
                bool active = false;

                foreach (var chapter in chapters)
                {
                    double singerCount = 0;

                    for (int i = 0; i < 10; i++)
                    {
                        singerCount++;
                        singerCount = singerCount % 4;

                        if (singerCount == 0)
                        {
                            active = false;
                        }
                        else
                        {
                            active = true;
                        }

                        var LastName = lastNames[random.Next(lastNames.Count)];
                        singers.Add(new Singer
                        {
                            FirstName = firstNames[random.Next(firstNames.Count)],
                            LastName = LastName,
                            DOB = new DateTime(2009 + random.Next(8), random.Next(1, 13), random.Next(1, 28)),
                            RegisterDate = DateTime.Now.AddMonths(-random.Next(1, 60)),
                            Address = $"{random.Next(100, 999)} {addresses[random.Next(addresses.Count)]}, {chapter.City}",
                            Status = active,
                            EmergencyContactFirstName = firstNames[random.Next(firstNames.Count)],
                            EmergencyContactLastName = LastName,
                            EmergencyContactPhone = $"555{random.Next(100, 999)}{random.Next(1000, 9999)}",
                            ChapterID = chapter.ID,
                            Chapter = chapter,
                        });
                    }
                }

                context.Singers.AddRange(singers);
                context.SaveChanges();


                var sessionNotes = new List<string>
                {
                    "The singers were full of energy today!",
                    "We practiced harmonizing, and everyone did great.",
                    "A few members had sore throats, so we took it easy.",
                    "Today’s warm-ups were fun and engaging!",
                    "One of the singers hit a high note for the first time!",
                    "A great rehearsal—our timing is getting better!",
                    "Someone forgot their lyrics, but we turned it into a learning moment.",
                    "We worked on stage presence and confidence.",
                    "Had a mini vocal battle—lots of fun and great practice!",
                    "The group was a bit shy today, but they warmed up quickly.",
                    "We introduced a new song, and everyone loved it!",
                    "A surprise visit from a professional singer made the day special.",
                    "We recorded our session and listened to our progress.",
                    "Some singers struggled with breath control, so we focused on exercises.",
                    "A fun improvisation session helped boost creativity!",
                    "Lots of laughter today—singing with joy is the best!",
                    "We had an impromptu solo performance from one of the singers.",
                    "Practiced singing in rounds—tricky but rewarding!",
                    "Worked on diction and pronunciation for clearer lyrics.",
                    "We learned about vocal health and how to avoid strain.",
                    "A power outage made us practice acapella—it was magical!",
                    "One singer was nervous but gained confidence by the end.",
                    "We ended the session with a group sing-along!",
                    "Tried singing with instruments for the first time—exciting!",
                    "Everyone gave their best effort today—so proud!"
                };

                var sessions = new List<Session>();

                foreach (var chapter in chapters)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        sessions.Add(new Session
                        {
                            Notes = sessionNotes[random.Next(sessionNotes.Count)],
                            Date = DateTime.Parse(DateTime.Now.AddDays(-random.Next(30, 1090)).ToShortDateString()),
                            ChapterID = chapter.ID
                        });
                    }
                }

                context.Sessions.AddRange(sessions);
                context.SaveChanges();

                // Seed SingerSessions
                var singerSessions = new List<SingerSession>();
                foreach (var session in sessions)
                {
                    var citySingers = singers.Where(s => s.ChapterID == session.ChapterID).Where(s => s.Status == true).OrderBy(x => random.Next()).Take(5).ToList();

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

                // Seed Volunteers
                var volunteers = new List<Volunteer>();
                int volunteerCount = volunteers.Count;
                    for (int i = 0; i < 20; i++)
                    {
                        if (volunteerCount > 0) //if already seeded, skip
                        {
                            return;
                        }
                        else //otherwise create new volunteers
                        {
                            var first = firstNames[random.Next(firstNames.Count)];
                            var last = lastNames[random.Next(lastNames.Count)];
                        volunteers.Add(new Volunteer
                        {
                            FirstName = first,
                            LastName = last,
                            Phone = $"{random.Next(100, 999)}-{random.Next(100, 999)}-{random.Next(1000, 9999)}",
                            Email = $"{first}{last}{random.Next(1, 999)}@email.com",
                            DOB = new DateTime(2011 + random.Next(8), random.Next(1, 13), random.Next(1, 28)),
                            RegisterDate = new DateTime(2023 + random.Next(-2, 2), DateTime.Now.Month, DateTime.Now.Day),
                            ChapterID = chapters.Any() ? chapters[random.Next(chapters.Count())].ID : 1
                            });
                        }
                    
                }
                context.Volunteers.AddRange(volunteers);
                context.SaveChanges();
            }
        }
    }
}