using Domain.Users;

namespace Tests.Data;

public static class UsersData
{
    public static User NewUser(string email, string userName, string passwordHash) => 
        new()
        {
            Id = Guid.NewGuid(),
            Email = email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = "testUserName",
            PasswordHash = passwordHash
        };
}