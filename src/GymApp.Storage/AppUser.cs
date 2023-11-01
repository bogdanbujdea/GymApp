using System.Collections.Generic;

namespace GymApp.Storage;

public class AppUser
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }
}