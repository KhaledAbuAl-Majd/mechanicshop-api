using MediatR;

namespace MechanicShop.Application.Common.Interfaces
{
    public interface IInvalidateCacheCommand
    {
        string[] Tags { get; }
    }
    public interface IInvalidateCacheCommand<TResponse> : IRequest<TResponse>, IInvalidateCacheCommand;
}
