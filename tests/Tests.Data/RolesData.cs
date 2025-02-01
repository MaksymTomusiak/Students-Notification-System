using Domain.Roles;

namespace Tests.Data;

public static class RolesData
{
    public static Role AdminRole =>
        new Role() { Id = Guid.NewGuid(), Name = "Admin"};
    
    public static Role UserRole =>
        new Role() { Id = Guid.NewGuid(), Name = "User"};
}