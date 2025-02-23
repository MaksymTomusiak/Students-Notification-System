using Domain.Feedbacks;
using Domain.Roles;
using Domain.Users;

namespace Api.Dtos;

public record UserDto(
    Guid Id,
    string UserName,
    string Email,
    string PhoneNumber,
    IList<string>? Roles)
{
    public static UserDto FromDomainModel(User user, IList<string>? roles = null)
        => new(user.Id,
            user.UserName, 
            user.Email, 
            user.PhoneNumber,
            roles ?? []);

}

public record UserLoginDto(
    string Email,
    string Password);
    
public record UserRegisterDto(
    string UserName,
    string Email,
    string Password);
    
public record UserUpdateDto(
    string PhoneNumber,
    string OldPassword,
    string NewPassword);