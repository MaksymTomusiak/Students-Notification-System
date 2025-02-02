using Domain.Roles;
using Domain.Users;

namespace Application.Common.Interfaces;

public interface IJwtProvider
{
    string Generate(User user, Role role);
}