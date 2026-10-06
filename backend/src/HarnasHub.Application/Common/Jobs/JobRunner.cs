#region Usings

using MediatR;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Common.Jobs;

/// <summary>Executes one job's request with its time limit and turns every way it can end — result, business error,
/// timeout, shutdown, unexpected exception — into a <see cref="JobOutcome"/> the worker can persist.</summary>
public static class JobRunner
{
	#region Public Methods

	/// <summary>Runs <paramref name="definition"/> on <paramref name="payloadJson"/>; never throws.</summary>
	public static async Task<JobOutcome> ExecuteAsync(
		IJobDefinition definition,
		string payloadJson,
		ISender sender,
		ILogger logger,
		CancellationToken stoppingToken)
	{
		using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
		timeout.CancelAfter(definition.Timeout);

		try
		{
			return await definition.ExecuteAsync(sender, payloadJson, timeout.Token);
		}
		catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
		{
			logger.LogWarning("Zadanie {Kind} przerwane zatrzymaniem serwera", definition.Kind);
			return JobOutcome.Failure(JobOutcome.ShutdownMessage);
		}
		catch (OperationCanceledException) when (timeout.IsCancellationRequested)
		{
			logger.LogWarning("Zadanie {Kind} przekroczyło limit czasu {Timeout}", definition.Kind, definition.Timeout);
			return JobOutcome.Failure(JobOutcome.TimeoutMessage);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Zadanie {Kind} zakończyło się nieoczekiwanym błędem", definition.Kind);
			return JobOutcome.Failure(JobOutcome.UnexpectedErrorMessage);
		}
	}

	#endregion
}
