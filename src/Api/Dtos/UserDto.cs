using Domain.Roles;
using Domain.Users;

namespace Api.Dtos;

public record UserDto(
    Guid Id,
    string UserName,
    string Email,
    IList<string>? Roles)
{
    public static UserDto FromDomainModel(User user, IList<string>? roles)
        => new(user.Id,
            user.UserName, 
            user.Email, 
            roles ?? []);

}