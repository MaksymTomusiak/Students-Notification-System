namespace Domain.Registers;

public record RegisterId(Guid Value)
{
    public static RegisterId Empty() => new(Guid.Empty);
    public static RegisterId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}