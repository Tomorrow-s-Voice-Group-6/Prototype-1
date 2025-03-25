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
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.AspNetCore.Identity;

namespace TVAttendance.Data
{
    public class TVInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider,
            bool DeleteDatabase = false, bool UseMigrations = true, bool SeedSampleData = true)
        {
            //var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            //var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            using (var context = new TomorrowsVoiceContext(
                serviceProvider.GetRequiredService<DbContextOptions<TomorrowsVoiceContext>>()))
            {
                var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
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
                    // Create 3 directors per city
                    for (int i = 0; i < 3; i++)
                    {
                        directors.Add(new Director
                        {
                            FirstName = firstNames[random.Next(firstNames.Count)],
                            LastName = lastNames[random.Next(lastNames.Count)],
                            DOB = new DateTime(1970 + random.Next(20), random.Next(1, 13), random.Next(1, 28)),
                            //HireDate = DateTime.Now.AddYears(-random.Next(1, 10)),
                            Address = $"{random.Next(100, 999)} {addresses[random.Next(addresses.Count)]}, {city}",
                            Email = $"{city}_director{random.Next(1000)}@example.com",
                            Phone = $"{random.Next(100, 999)}{random.Next(100, 999)}{random.Next(1000, 9999)}",
                            Status = true
                        });
                    }
                }


                context.Directors.AddRange(directors);
                context.SaveChanges();

                // Seed Chapters
                var chapters = new List<Chapter>();
                int directorsPerChapter = 3;  // Change to 3 directors per chapter

                var validLetters = "ABCEGHJNPRSTVXY";

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

                    // Find directors who belong to the current city
                    var directorsForCity = directors.Where(d => d.Address.Contains(city)).ToList();

                    // Select 3 directors for this chapter
                    while (chapterDirectors.Count < directorsPerChapter)
                    {
                        var randomDirector = directorsForCity[random.Next(directorsForCity.Count)];

                        if (!assignedDirectors.Contains(randomDirector.ID)) // Ensure uniqueness
                        {
                            chapterDirectors.Add(randomDirector);
                            assignedDirectors.Add(randomDirector.ID);
                        }
                    }

                    var provinceList = Enum.GetValues(typeof(Province)).Cast<Province>().ToList();
                    var selectedProvince = provinceList[random.Next(provinceList.Count)];

                    // Create the chapter and assign the selected directors
                    chapters.Add(new Chapter
                    {
                        City = city,
                        Street = $"{random.Next(100, 999)} {addresses[random.Next(addresses.Count)]}, {city}",
                        Province = selectedProvince,
                        ZipCode = GeneratePostalCode(),
                        Directors = chapterDirectors // Assign directors to this chapter
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
                            Chapter = chapter
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
                    ("Charlottetown", "777 Queen St", Province.PrinceEdwardIsland),
                    ("Charlottetown", "731 Queen St", Province.PrinceEdwardIsland),
                    ("Charlottetown", "254 Queen St", Province.PrinceEdwardIsland),

    
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
                    Province.PrinceEdwardIsland
                };


                var eventNames = new List<string> { "Fundraiser", "Workshops", "Webinars", "Giftwrapping" };
                int volunteerCap = random.Next(3, 7);

                // Generation of events
                for (int i = 0; i < 150; i++)
                {
                    var cityAndStreet = citiesAndStreets[random.Next(citiesAndStreets.Count)];
                    var city = cityAndStreet.City;
                    var street = cityAndStreet.Street;
                    var province = cityAndStreet.Province;
                    var eventName = eventNames[random.Next(eventNames.Count)];

                    int randomStatus = random.Next(0, 5);
                    bool status;

                    if (randomStatus < 1)
                    {
                        status = false;
                    }
                    else
                    {
                        status = true;
                    }

                    DateTime eventStart;
                    DateTime eventEnd;
                    bool isFutureEvent = i < 75; // First 100 events will be in the future

                    //if (eventName == "Giftwrapping")
                    //{
                    //    // Ensure the event is in December
                    //    int year = isFutureEvent ? DateTime.Now.Year : DateTime.Now.Year - random.Next(1, 3);
                    //    int day = random.Next(1, 24); // Any day in December before 25th
                    //    eventStart = new DateTime(year, 12, day).AddHours(9);
                    //    eventEnd = eventStart.AddDays(random.Next(1, 5)).AddHours(random.Next(4, 8));
                    //}
                    //else
                    //{
                    if (isFutureEvent)
                    {
                        // Set future events (up to 1 year in the future)
                        eventStart = DateTime.Now.AddDays(random.Next(0, 30)).AddHours(9);
                        eventEnd = eventStart.AddDays(random.Next(1, 7)).AddHours(random.Next(4, 8)); // End 1-10 days after the start

                    }
                    else
                    {
                        // Set past events (up to 3 years in the past)
                        eventStart = DateTime.Now.AddDays(random.Next(-365 * 3, -1)).AddHours(9); // Up to 3 years in the past
                        eventEnd = eventStart.AddDays(random.Next(1, 6)).AddHours(random.Next(4, 8)); // End 1-10 days after the start
                        status = false;
                    }
                    //}

                    // **Check if an event with the same name and street already exists**
                    bool eventExists = context.Events.Where(e => e.EventName == eventName && e.EventStreet == street).Count() > 0;

                    var newEvent = new Event
                    {
                        EventName = eventName,
                        EventStreet = street,
                        EventCity = city,
                        EventPostalCode = GenerateRandomPostalCode(),
                        EventProvince = province,
                        EventStart = eventStart,
                        EventEnd = eventEnd,
                        EventOpen = status,
                        VolunteerCapacity = volunteerCap
                    };

                    try
                    {
                        if (!eventExists)
                        {
                            context.Events.Add(newEvent);
                            context.SaveChanges();
                        }
                    }
                    catch
                    {
                        context.Events.Remove(newEvent);
                    }

                }
                // Save changes to the database

                var reason = new List<AttendanceReason?>
                {
                    AttendanceReason.Illness,
                    AttendanceReason.Transportation,
                    AttendanceReason.FamilyEmergency,
                    AttendanceReason.PriorCommitment,
                    AttendanceReason.RelativePassed,
                    AttendanceReason.WorkObligations,
                    AttendanceReason.Other
                };

                int[] volunteerIDs = context.Volunteers.Select(a => a.ID).ToArray();
                int volunteerIDCount = volunteerIDs.Length;

                var eventList = context.Events.ToList();

                foreach (var eventObj in eventList)
                {
                    DateTime eventStartDate = eventObj.EventStart.Date;

                    for (int c = 0; eventStartDate.AddDays(c).CompareTo(eventObj.EventEnd.Date) <= 0; c++)
                    {
                        DateTime shiftStart = eventStartDate.AddDays(c);

                        for (int i = 0; i < eventObj.VolunteerCapacity; i++)
                        {
                            shiftStart = shiftStart.AddHours(random.Next(8, 13));
                            DateTime shiftEnd = shiftStart.AddHours(random.Next(3, 8));

                            var shift = new Shift
                            {
                                EventID = eventObj.ID,  // Only store EventID
                                ShiftStart = shiftStart,
                                ShiftEnd = shiftEnd
                            };

                            int selectedID = volunteerIDs[random.Next(volunteerIDCount)];

                            bool? attended = null;
                            DateTime? shiftClockIn = null;
                            DateTime? shiftClockOut = null;

                            AttendanceReason? AttendReason = null;

                            if (shiftStart.Date.CompareTo(DateTime.Now.Date) < 0)
                            {
                                if (random.Next(0, 1) == 1)
                                {
                                    attended = false;
                                    AttendReason = reason[random.Next(0, reason.Count)];
                                }
                                else
                                {
                                    attended = true;
                                    shiftClockIn = eventStartDate.AddHours(random.Next(8, 10));
                                    shiftClockOut = shiftClockIn.Value.AddTicks(shiftEnd.Ticks);
                                }
                            }

                            shift.ShiftVolunteers.Add(new ShiftVolunteer
                            {
                                ShiftID = shift.ID,
                                ClockIn = shiftClockIn,
                                ClockOut = shiftClockOut,
                                NonAttendance = attended,
                                AttendanceReason = AttendReason,
                                VolunteerID = selectedID
                            });

                            try
                            {
                                if (!(volunteers.Where(v => v.ID == selectedID)
                                    .SelectMany(v => v.ShiftVolunteers)
                                    .Any(s => s.Shift.ShiftStart == shiftStart)))
                                {
                                    context.Shifts.AddRange(shift);
                                    context.SaveChanges();
                                }
                            }
                            catch
                            {
                                context.Shifts.Remove(shift);
                            }
                        }

                        //if (i == 0)
                        //{
                        //    shiftDate = DateOnly.FromDateTime(eventObj.EventStart.Date);
                        //}
                        //else 
                        //{
                        //    shiftDate = DateOnly.Parse(randomEventDate.ToShortDateString());
                        //}



                    }
                }

                // Seed roles
                string[] roleNames = { "Admin", "Supervisor", "Director", "User" };

                foreach (var roleName in roleNames)
                {
                    var role = await roleManager.FindByNameAsync(roleName);
                    if (role == null)
                    {
                        // Create the role if it doesn't exist
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Define users with roles
                var users = new[]
                {
                    new { Email = "admin@gmail.com", Username = "admin@gmail.com", Password = "AdminPassword123!", Role = "Admin", DisplayName = "Admin" },
                    new { Email = "supervisor@gmail.com", Username = "supervisor@gmail.com", Password = "SupervisorPassword123!", Role = "Supervisor", DisplayName = "Supervisor User" },
                    new { Email = "director@gmail.com", Username = "director@gmail.com", Password = "DirectorPassword123!", Role = "Director", DisplayName = "Director User" },
                    new { Email = "user@gmail.com", Username = "user@gmail.com", Password = "UserPassword123!", Role = "User", DisplayName = "Steven" },
                };

                foreach (var u in users)
                {
                    var user = await userManager.FindByEmailAsync(u.Email);
                    if (user == null)
                    {
                        // Create user if it doesn't exist
                        user = new IdentityUser
                        {
                            UserName = u.Username,  // Identity expects UserName to be unique; using Email is safest
                            Email = u.Email,
                            EmailConfirmed = true,  // Ensure email is confirmed
                        };

                        // Create the user
                        var result = await userManager.CreateAsync(user, u.Password);
                        if (result.Succeeded)
                        {
                            if (result.Succeeded)
                            {
                                // Assign the user to the role
                                var roleResult = await userManager.AddToRoleAsync(user, u.Role);
                                if (!roleResult.Succeeded)
                                {
                                    Console.WriteLine($"Error assigning role to {u.Email}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Error creating user {u.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                            }

                        }
                        else
                        {
                            Console.WriteLine($"Error creating user {u.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"User {u.Email} already exists.");
                    }
                }


                var user2 = await userManager.FindByEmailAsync("admin@gmail.com");
                if (user2 != null)
                {
                    bool valid = await userManager.CheckPasswordAsync(user2, "AdminPassword123!");
                    Console.WriteLine($"Password valid: {valid}");
                    bool valid2 = await userManager.CheckPasswordAsync(user2, "AdminPassword123");
                    Console.WriteLine($"Password valid: {valid2}");
                }
                else
                {
                    Console.WriteLine("User not found.");
                }
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

        

        //static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        //{
        //    string[] roleNames = { "Admin", "Supervisor", "User" };

        //    foreach (var roleName in roleNames)
        //    {
        //        var role = await roleManager.FindByNameAsync(roleName);
        //        if (role == null)
        //        {
        //            await roleManager.CreateAsync(new IdentityRole(roleName));
        //        }
        //    }
        //}

        //public static async Task SeedUsersAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        //{
        //    // Ensure roles exist before assigning them to users
        //    await SeedRolesAsync(roleManager);

        //    // Seed Admin User
        //    var adminUser = await userManager.FindByEmailAsync("admin@gmail.com");
        //    if (adminUser == null)
        //    {
        //        adminUser = new IdentityUser
        //        {
        //            UserName = "Admin",
        //            NormalizedUserName = "ADMIN",
        //            Email = "admin@gmail.com",
        //            NormalizedEmail = "ADMIN@GMAIL.COM",
        //            EmailConfirmed = true
        //        };

        //        var result = await userManager.CreateAsync(adminUser, "AdminPassword123!");
        //        if (result.Succeeded)
        //        {
        //            await userManager.AddToRoleAsync(adminUser, "Admin");
        //        }
        //    }

        //    // Seed Supervisor User
        //    var supervisorUser = await userManager.FindByEmailAsync("supervisor@gmail.com");
        //    if (supervisorUser == null)
        //    {
        //        supervisorUser = new IdentityUser
        //        {
        //            UserName = "Supervisor",
        //            NormalizedUserName = "SUPERVISOR",
        //            Email = "supervisor@gmail.com",
        //            NormalizedEmail = "SUPERVISOR@GMAIL.COM",
        //            EmailConfirmed = true
        //        };

        //        var result = await userManager.CreateAsync(supervisorUser, "SupervisorPassword123!");
        //        if (result.Succeeded)
        //        {
        //            await userManager.AddToRoleAsync(supervisorUser, "Supervisor");
        //        }
        //    }

        //    // Seed Regular User
        //    var regularUser = await userManager.FindByEmailAsync("user@gmail.com");
        //    if (regularUser == null)
        //    {
        //        regularUser = new IdentityUser
        //        {
        //            UserName = "Fred",
        //            NormalizedUserName = "FRED",
        //            Email = "user@gmail.com",
        //            NormalizedEmail = "USER@GMAIL.COM",
        //            EmailConfirmed = true
        //        };

        //        var result = await userManager.CreateAsync(regularUser, "UserPassword123!");
        //        if (result.Succeeded)
        //        {
        //            await userManager.AddToRoleAsync(regularUser, "User");
        //        }
        //    }
        //}
    }
}

