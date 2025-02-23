using Domain.CourseBans;

namespace Api.Dtos;

public record CourseBanDto(
    Guid Id,
    Guid UserId,
    Guid CourseId,
    DateTime BannedAt,
    string Reason,
    CourseDto? Course = null)
{
    public static CourseBanDto FromDomainModel(CourseBan ban) 
        => new(
            ban.Id.Value,
            ban.UserId,
            ban.CourseId.Value,
            ban.BannedAt,
            ban.Reason,
            ban.Course == null ? null : CourseDto.FromDomainModel(ban.Course));
}

public record BanDto(
    Guid UserId,
    Guid CourseId,
    string Reason);