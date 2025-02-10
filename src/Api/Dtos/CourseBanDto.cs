using Domain.CourseBans;

namespace Api.Dtos;

public record CourseBanDto(
    Guid Id,
    Guid UserId,
    Guid CourseId,
    DateTime BannedAt,
    string Reason)
{
    public static CourseBanDto FromDomainModel(CourseBan ban) 
        => new(ban.Id.Value, ban.UserId, ban.CourseId.Value, ban.BannedAt, ban.Reason);
}

public record BanDto(
    Guid UserId,
    Guid CourseId,
    string Reason);
    
public record UnbanDto(
    Guid UserId,
    Guid CourseId);