using Domain.Feedbacks;
using Domain.Registers;
using Microsoft.AspNetCore.Identity;

namespace Domain.Users;

public class User : IdentityUser<Guid>
{
    public ICollection<Register> Registers { get; set; } = new List<Register>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}