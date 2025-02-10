namespace Domain.CourseBans;

public record CourseBanId(Guid Value)
{
    public static CourseBanId Empty() => new(Guid.Empty);
    public static CourseBanId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}