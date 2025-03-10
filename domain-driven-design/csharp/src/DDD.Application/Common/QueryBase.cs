namespace DDD.Application.Common;

using MediatR;

public abstract record QueryBase<TResponse> : IRequest<TResponse>;
