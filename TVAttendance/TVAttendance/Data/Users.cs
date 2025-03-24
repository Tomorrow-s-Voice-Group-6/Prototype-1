//using Microsoft.AspNetCore.Identity;
//using System;
//using System.Threading.Tasks;

//public static class Users
//{
//    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
//    {
//        string[] roleNames = { "Admin", "Supervisor", "User" };

//        foreach (var roleName in roleNames)
//        {
//            var role = await roleManager.FindByNameAsync(roleName);
//            if (role == null)
//            {
//                await roleManager.CreateAsync(new IdentityRole(roleName));
//            }
//        }
//    }

//    public static async Task SeedUsersAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
//    {
//        await SeedRolesAsync(roleManager);

//        var users = new[]
//        {
//        new { Email = "admin@gmail.com", Username = "Admin", Password = "AdminPassword123!", Role = "Admin" },
//        new { Email = "supervisor@gmail.com", Username = "Supervisor", Password = "SupervisorPassword123!", Role = "Supervisor" },
//        new { Email = "user@gmail.com", Username = "Fred", Password = "UserPassword123!", Role = "User" }
//    };

//        foreach (var u in users)
//        {
//            var user = await userManager.FindByEmailAsync(u.Email);
//            if (user == null)
//            {
//                user = new IdentityUser
//                {
//                    UserName = u.Username,
//                    NormalizedUserName = u.Username.ToUpper(),
//                    Email = u.Email,
//                    NormalizedEmail = u.Email.ToUpper(),
//                    EmailConfirmed = true,  // Ensure the email is confirmed
//                };

//                var result = await userManager.CreateAsync(user, u.Password);
//                if (result.Succeeded)
//                {
//                    await userManager.AddToRoleAsync(user, u.Role);
//                    // Explicitly confirm the email after creation
//                    await userManager.UpdateAsync(user);
//                }
//                else
//                {
//                    Console.WriteLine($"Error creating user {u.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
//                }
//            }
//        }
//    }
//}
