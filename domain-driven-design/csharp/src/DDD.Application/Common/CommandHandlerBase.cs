namespace DDD.Application.Common;

using DDD.Infrastructure.CrossCuttingConcerns.Transactions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public abstract class CommandHandlerBase<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : CommandBase<TResponse>
{
    protected readonly IUnitOfWork UnitOfWork;
    
    protected CommandHandlerBase(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }
    
    public abstract Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
}