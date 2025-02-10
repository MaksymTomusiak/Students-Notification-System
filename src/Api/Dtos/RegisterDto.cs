using Domain.Registers;

namespace Api.Dtos;

public record RegisterDto(
    Guid Id,
    Guid UserId,
    Guid CourseId,
    DateTime RegisteredAt,
    CourseDto? Course = null)
{
    public static RegisterDto FromDomainModel(Register register)
        => new(
            register.Id.Value,
            register.UserId,
            register.CourseId.Value,
            register.RegisteredAt,
            register.Course == null ? null : CourseDto.FromDomainModel(register.Course)
            );
}