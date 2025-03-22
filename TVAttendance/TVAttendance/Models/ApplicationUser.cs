using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    // Add additional custom properties for your application user
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
