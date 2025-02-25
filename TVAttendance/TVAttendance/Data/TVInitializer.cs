using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TVAttendance.Models;
using TVAttendance.Data;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;
using TVAttendance.Data.Migrations;
using Microsoft.Extensions.Logging;

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

                var streetNames = new List<string> { "Maple", "Oak", "Pine", "Birch", "Elm", "Cedar", "Willow", "Rosewood", "Riverbend",
                    "Highland", "Sunset", "Forest", "Lakeview", "Hilltop", "Chestnut", "Ivy", "Juniper", "Magnolia", "Cherry Blossom", "Bluebird",
                    "Silver Creek", "Redwood", "Greenfield", "Autumn", "Crosswinds", "Diamond", "Maplewood",
                    "Golden Gate", "Tanglewood", "Shadowbrook" };

                var streetTypes = new List<string>
                {
                   "St", "Rd", "Ave", "Blvd", "Ln", "Cres", "Dr", "Pkwy"
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
                int directorsPerChapter = 1;

                var validLetters = "ABCEGHJ-NPRSTVXY";

                string GeneratePostalCode()
                {
                    return $"{validLetters[random.Next(validLetters.Length)]}{random.Next(10)}" +
                           $"{validLetters[random.Next(validLetters.Length)]}{random.Next(10)}" +
                           $"{validLetters[random.Next(validLetters.Length)]}{random.Next(10)}";
                }

                var assignedDirectors = new HashSet<int>(); // Track assigned director IDs

                foreach (string city in cities)
                {
                    var chapterDirectors = new List<Director>();

                    while (chapterDirectors.Count < directorsPerChapter)
                    {
                        var randomDirector = directors[random.Next(directors.Count)];

                        if (!assignedDirectors.Contains(randomDirector.ID)) // Ensure uniqueness
                        {
                            chapterDirectors.Add(randomDirector);
                            assignedDirectors.Add(randomDirector.ID);
                        }
                    }

                    chapters.Add(new Chapter
                    {
                        City = city,
                        Street = $"{random.Next(100, 999)} {addresses[random.Next(addresses.Count)]}, {city}",
                        ZipCode = GeneratePostalCode(),
                        Directors = chapterDirectors
                    });
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
                            Status = active,
                            Street = $"{random.Next(10, 999)} {streetNames[random.Next(streetNames.Count)]} {streetTypes[random.Next(streetTypes.Count)]}",
                            City = cities[random.Next(0, 5)],
                            Province = Province.Ontario,
                            PostalCode = "A9A2B2",
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
                    for (int i = 0; i < 50; i++)
                    {
                        if (volunteerCount > 10) //if already seeded, skip
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
                            });
                        }
                    
                }
                context.Volunteers.AddRange(volunteers);
                context.SaveChanges();


                // List of Canadian cities and streets, organized by province
                var citiesAndStreets = new List<(string City, string Street, Province Province)>
                {
                    // Ontario
                    ("Toronto", "123 Queen St W", Province.Ontario),
                    ("Toronto", "456 King St E", Province.Ontario),
                    ("Toronto", "789 Bay St", Province.Ontario),
                    ("Ottawa", "202 Sparks St", Province.Ontario),
                    ("Ottawa", "333 Rideau St", Province.Ontario),
                    ("Mississauga", "44 City Centre Dr", Province.Ontario),
                    ("Brampton", "456 Steeles Ave W", Province.Ontario),
                    ("Hamilton", "777 Main St W", Province.Ontario),
                    ("London", "555 Oxford St E", Province.Ontario),
    
                    // British Columbia
                    ("Vancouver", "456 Robson St", Province.BritishColumbia),
                    ("Vancouver", "123 Granville St", Province.BritishColumbia),
                    ("Victoria", "707 Douglas St", Province.BritishColumbia),
                    ("Surrey", "101 96 Ave", Province.BritishColumbia),
                    ("Burnaby", "322 Metrotown Blvd", Province.BritishColumbia),
                    ("Richmond", "512 No. 3 Rd", Province.BritishColumbia),
                    ("Kelowna", "788 Bernard Ave", Province.BritishColumbia),
    
                    // Quebec
                    ("Montreal", "789 Rue Saint-Denis", Province.Quebec),
                    ("Montreal", "1000 Rue Sainte-Catherine O", Province.Quebec),
                    ("Quebec City", "606 Grande Allée E", Province.Quebec),
                    ("Quebec City", "555 Rue Saint-Jean", Province.Quebec),
                    ("Gatineau", "455 Boulevard de la Gappe", Province.Quebec),
                    ("Trois-Rivières", "700 Boulevard des Forges", Province.Quebec),
    
                    // Alberta
                    ("Calgary", "101 17th Ave SW", Province.Alberta),
                    ("Calgary", "2000 14th St NW", Province.Alberta),
                    ("Edmonton", "303 Jasper Ave NW", Province.Alberta),
                    ("Edmonton", "121 Street NW", Province.Alberta),
                    ("Red Deer", "5000 50th Ave", Province.Alberta),
                    ("Lethbridge", "310 5 St S", Province.Alberta),
    
                    // Nova Scotia
                    ("Halifax", "505 Barrington St", Province.NovaScotia),
                    ("Halifax", "2000 Robie St", Province.NovaScotia),
                    ("Sydney", "123 George St", Province.NovaScotia),
                    ("Dartmouth", "456 Portland St", Province.NovaScotia),
    
                    // New Brunswick
                    ("Fredericton", "101 Queen St", Province.NewBrunswick),
                    ("Moncton", "600 Main St", Province.NewBrunswick),
                    ("Saint John", "303 King St", Province.NewBrunswick),
    
                    // Manitoba
                    ("Winnipeg", "404 Portage Ave", Province.Manitoba),
                    ("Winnipeg", "123 Broadway", Province.Manitoba),
                    ("Brandon", "2000 18th St", Province.Manitoba),
    
                    // Saskatchewan
                    ("Saskatoon", "233 3rd Ave S", Province.Saskatchewan),
                    ("Regina", "444 Albert St", Province.Saskatchewan),
    
                    // Newfoundland and Labrador
                    ("St. John's", "100 Water St", Province.NewfoundlandAndLabrador),
                    ("St. John's", "456 Kenmount Rd", Province.NewfoundlandAndLabrador),
    
                    // Prince Edward Island
                    ("Charlottetown", "777 Queen St", Province.PEI),
    
                    // Other cities from various provinces (for more variety)
                    ("Kitchener", "500 King St W", Province.Ontario),
                    ("Waterloo", "300 University Ave W", Province.Ontario),
                    ("Kelowna", "1500 Water St", Province.BritishColumbia),
                    ("Surrey", "12000 80 Ave", Province.BritishColumbia),
                    ("Mont-Tremblant", "1055 Rue des Voyageurs", Province.Quebec),
                    ("Banff", "200 Banff Ave", Province.Alberta)
                };

                // List of Canadian provinces
                var provinces = new List<Province>
                {
                    Province.Ontario, // Enum mapping
                    Province.BritishColumbia,
                    Province.Quebec,
                    Province.Alberta,
                    Province.NovaScotia,
                    Province.NewBrunswick,
                    Province.Manitoba,
                    Province.Saskatchewan,
                    Province.NewfoundlandAndLabrador,
                    Province.PEI
                };


                var eventNames = new List<string> { "Fundraiser", "Workshops", "Webinars", "Giftwrapping" };


                // Generation of events
                for (int i = 0; i < 10; i++)
                {
                    var cityAndStreet = citiesAndStreets[random.Next(citiesAndStreets.Count)];
                    var city = cityAndStreet.City;
                    var street = cityAndStreet.Street;
                    var province = cityAndStreet.Province;
                    var eventName = eventNames[random.Next(eventNames.Count)];

                    DateTime eventStart;
                    DateTime eventEnd;
                    bool isFutureEvent = i < 5; // First 5 events will be in the future

                    if (eventName == "Giftwrapping")
                    {
                        // Ensure the event is in December
                        int year = isFutureEvent ? DateTime.Now.Year : DateTime.Now.Year - random.Next(1, 3);
                        int day = random.Next(1, 24); // Any day in December before 25th
                        eventStart = new DateTime(year, 12, day);
                        eventEnd = eventStart.AddDays(random.Next(1, 5));
                    }
                    else
                    {
                        if (isFutureEvent)
                        {
                            // Set future events (up to 1 year in the future)
                            eventStart = DateTime.Now.AddDays(random.Next(100, 365)); // Up to 1 year in the future
                            eventEnd = eventStart.AddDays(random.Next(1, 10)); // End 1-10 days after the start
                        }
                        else
                        {
                            // Set past events (up to 3 years in the past)
                            eventStart = DateTime.Now.AddDays(random.Next(-365 * 3, -1)); // Up to 3 years in the past
                            eventEnd = eventStart.AddDays(random.Next(1, 10)); // End 1-10 days after the start
                        }
                    }

                    // **Check if an event with the same name and street already exists**
                    bool eventExists = context.Events.Any(e => e.EventName == eventName && e.EventStreet == street);

                    if (!eventExists)
                    {
                        var newEvent = new Event
                        {
                            EventName = eventName,
                            EventStreet = street,
                            EventCity = city,
                            EventPostalCode = GenerateRandomPostalCode(),
                            EventProvince = province,
                            EventStart = eventStart,
                            EventEnd = eventEnd
                        };

                        context.Events.AddRange(newEvent);
                    }
                }

                // Save changes to the database
                context.SaveChanges();




                // List of possible attendance reasons
                var nonAttendanceReasons = new List<string>
                {
                    "Sick",
                    "Emergency",
                    "Family issues",
                    "Work conflict",
                    "Transportation issues",
                    "Personal reasons",
                    "Other"
                };


                // Seed VolunteerEvent instances
                for (int i = 0; i < volunteers.Count; i++)
                {
                    var volunteer = volunteers[i];

                    // Get only past events (those that have ended before the current time)
                    var pastEvents = context.Events.Where(e => e.EventEnd < DateTime.Now).ToList();

                    if (!pastEvents.Any()) continue; // Skip if no past events exist

                    // Randomly decide how many events to assign to this volunteer (between 1 and 3)
                    int numberOfEventsToAssign = random.Next(1, Math.Min(4, pastEvents.Count + 1));

                    // Randomly shuffle and take the first N past events for this volunteer
                    var assignedEvents = pastEvents.OrderBy(e => random.Next()).Take(numberOfEventsToAssign).ToList();

                    foreach (var eventAssigned in assignedEvents)
                    {
                        // Ensure ShiftStart is within the event duration
                        var shiftStart = eventAssigned.EventStart.AddHours(random.Next(0, (eventAssigned.EventEnd - eventAssigned.EventStart).Hours));
                        var shiftEnd = shiftStart.AddHours(random.Next(1, Math.Max(2, (eventAssigned.EventEnd - shiftStart).Hours)));

                        var volunteerEvent = new VolunteerEvent
                        {
                            EventID = eventAssigned.ID,  // Only store EventID
                            VolunteerID = volunteer.ID,  // Only store VolunteerID
                            ShiftStart = shiftStart,
                            ShiftEnd = shiftEnd,
                            ShiftAttended = random.NextDouble() >= 0.3,  // 70% chance of attending
                            NonAttendanceNote = null
                        };

                        // If the volunteer did not attend, assign a random non-attendance reason
                        if (!volunteerEvent.ShiftAttended)
                        {
                            volunteerEvent.NonAttendanceNote = nonAttendanceReasons[random.Next(nonAttendanceReasons.Count)];
                        }

                        // Save the volunteerEvent to the database (or further processing)
                        context.VolunteerEvents.AddRange(volunteerEvent);

                    }
                }

                // Save changes to the database
                context.SaveChanges();

            }


            // Helper method to generate a random Canadian postal code
            static string GenerateRandomPostalCode()
            {
                var random = new Random();
                string[] validStartingLetters = new string[] { "A", "B", "C", "E", "G", "H", "J", "K", "L", "M", "N", "P", "R", "S", "T", "V", "X", "Y" };
                string letter1 = validStartingLetters[random.Next(validStartingLetters.Length)];
                string letter2 = validStartingLetters[random.Next(validStartingLetters.Length)];
                string letter3 = validStartingLetters[random.Next(validStartingLetters.Length)];
                return $"{letter1}{random.Next(1, 10)}{letter2}{random.Next(1, 10)}{letter3}{random.Next(1, 10)}";
            }
        }

    }
}