namespace HeroRegistry.Security;

// Course only: a real application keeps accounts in ASP.NET Core Identity or Entra ID.
public sealed class CourseUser
{
    public string Name { get; set; } = "";
    public string Password { get; set; } = "";
    public string[] Roles { get; set; } = [];
}

public sealed class CourseUsers(IConfiguration configuration)
{
    private readonly List<CourseUser> _users =
        configuration.GetSection("Users").Get<List<CourseUser>>() ?? [];

    public CourseUser? Find(string name, string password) =>
        _users.FirstOrDefault(u =>
            string.Equals(u.Name, name, StringComparison.OrdinalIgnoreCase)
            && u.Password == password);
}
