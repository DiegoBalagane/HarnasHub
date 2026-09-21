using MediatR;

namespace HarnasHub.Tests.Common;

/// <summary>Stub <see cref="ISender"/> that returns a fixed response to every <see cref="Send{TResponse}"/> call —
/// for handlers that delegate to another handler via MediatR instead of calling it directly, without spinning up
/// the full MediatR pipeline in a test.</summary>
public class TestSender<TResponse>(TResponse response) : ISender
{
	#region Public Methods

	public Task<TResponse1> Send<TResponse1>(IRequest<TResponse1> request, CancellationToken cancellationToken = default) =>
		Task.FromResult((TResponse1)(object)response!);

	public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest =>
		throw new NotImplementedException();

	public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
		throw new NotImplementedException();

	public IAsyncEnumerable<TResponse1> CreateStream<TResponse1>(IStreamRequest<TResponse1> request, CancellationToken cancellationToken = default) =>
		throw new NotImplementedException();

	public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
		throw new NotImplementedException();

	#endregion
}
