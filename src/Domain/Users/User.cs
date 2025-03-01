using Domain.CourseBans;
using Domain.Feedbacks;
using Domain.Registers;
using Microsoft.AspNetCore.Identity;

namespace Domain.Users;

public class User : IdentityUser<Guid>
{
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiration { get; set; }
    public ICollection<IdentityUserRole<Guid>> UserRoles { get; set; } = new List<IdentityUserRole<Guid>>();
    public ICollection<Register> Registers { get; set; } = new List<Register>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public ICollection<CourseBan> CourseBans { get; set; } = new List<CourseBan>();
}