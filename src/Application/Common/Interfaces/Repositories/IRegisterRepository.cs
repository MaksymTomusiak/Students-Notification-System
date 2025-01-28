using Domain.Registers;

namespace Application.Common.Interfaces.Repositories;

public interface IRegisterRepository
{
    Task<Register> Add(Register register, CancellationToken cancellationToken);
    Task<Register> Update(Register register, CancellationToken cancellationToken);
    Task<Register> Delete(Register register, CancellationToken cancellationToken);
}