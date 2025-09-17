// InMemoryUserStore.cs
using System.Collections.Generic;
using TodoApi.Models;

public static class InMemoryUserStore
{
    public static List<User> Users = new()
    {
        new User { Username = "admin", Password = "admin123", Role = "Admin" },
        new User { Username = "user", Password = "user123", Role = "User" }
    };
}
