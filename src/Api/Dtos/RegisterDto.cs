using Domain.Registers;

namespace Api.Dtos;

public record RegisterDto(
    Guid Id,
    Guid UserId,
    Guid CourseId,
    DateTime RegisteredAt)
{
    public static RegisterDto FromDomainModel(Register register)
        => new(register.Id.Value, register.UserId, register.CourseId.Value, register.RegisteredAt);
}