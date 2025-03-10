namespace DDD.Application.Common;

using MediatR;

public abstract record CommandBase<TResponse> : IRequest<TResponse>;
