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
            UserName = userName,
            PasswordHash = passwordHash
        };
    
    public static User MainUser() => 
        new()
        {
            Id = Guid.NewGuid(),
            Email = "mainUser@gmail.com",
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = "mainUserName"
        };
    public static User SecondaryUser() => 
        new()
        {
            Id = Guid.NewGuid(),
            Email = "secondaryUser@gmail.com",
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = "secondaryUserName"
        };
    
    public static User AdminUser() => 
        new()
        {
            Id = Guid.NewGuid(),
            Email = "testAdminUser@gmail.com",
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = "testAdminUserName"
        };
}