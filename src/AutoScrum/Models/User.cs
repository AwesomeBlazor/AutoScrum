namespace AutoScrum.Models;

public class User
{
    public User() { }
    public User(string displayName, string email)
    {
        DisplayName = displayName;
        Email = email;
    }

    public string DisplayName { get; set; }
    public string Email { get; set; }

    public string Blocking { get; set; }
    public bool IncludeUser { get; set; } = true;
}
