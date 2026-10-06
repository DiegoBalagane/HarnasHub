#region Usings

using System.Text.Json;
using ErrorOr;
using FluentValidation;
using HarnasHub.Application.Common.Jobs;
using HarnasHub.Application.Features.MatchAnalysis.AttachDemoToResult;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Common.Jobs;

public class JobDefinitionTests
{
	#region Private Fields

	private static readonly JobDefinition<AttachDemoToResultCommand, MatchDemoAnalysisDto> Definition =
		new("test.attach", JobLane.Demo, "Start", TimeSpan.FromMinutes(1));

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_validate_with_the_request_validator()
	{
		var services = new ServiceCollection()
			.AddSingleton<IValidator<AttachDemoToResultCommand>, AttachDemoToResultCommandValidator>()
			.BuildServiceProvider();

		var errors = await Definition.ValidateAsync(new AttachDemoToResultCommand(Guid.NewGuid(), "../etc/passwd"), services, CancellationToken.None);

		var error = Assert.Single(errors);
		Assert.Equal(ErrorType.Validation, error.Type);
	}

	[Fact]
	public async Task Should_round_trip_the_payload_and_serialize_the_result_in_camel_case()
	{
		var matchId = Guid.NewGuid();
		var command = new AttachDemoToResultCommand(matchId, "demos/0123456789abcdef0123456789abcdef");
		var payload = Definition.SerializePayload(command);
		var sender = new TestSender<ErrorOr<MatchDemoAnalysisDto>>(new MatchDemoAnalysisDto(matchId, 24, 4, true));

		var outcome = await Definition.ExecuteAsync(sender, payload, CancellationToken.None);

		Assert.True(outcome.Succeeded);
		using var result = JsonDocument.Parse(outcome.ResultJson!);
		Assert.Equal(24, result.RootElement.GetProperty("roundsCount").GetInt32());
		Assert.True(result.RootElement.GetProperty("ourTeamResolved").GetBoolean());
	}

	[Fact]
	public async Task Should_turn_a_business_error_into_a_failure_with_its_polish_message()
	{
		var payload = Definition.SerializePayload(new AttachDemoToResultCommand(Guid.NewGuid(), "demos/0123456789abcdef0123456789abcdef"));
		var sender = new TestSender<ErrorOr<MatchDemoAnalysisDto>>(Error.NotFound("Results.MatchNotFound", "Nie znaleziono meczu."));

		var outcome = await Definition.ExecuteAsync(sender, payload, CancellationToken.None);

		Assert.False(outcome.Succeeded);
		Assert.Equal("Nie znaleziono meczu.", outcome.ErrorMessage);
	}

	#endregion
}
