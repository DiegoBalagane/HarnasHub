using System.Globalization;
using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Results.AddResult;
using HarnasHub.Application.Features.Results.GetResults;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Http.Features;

namespace HarnasHub.Api.Endpoints.MatchResults;

/// <summary>Match/scrim result endpoints under /api/results.</summary>
public class MatchResultsEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/results").WithTags("Results").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetResultsQuery(), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		// multipart/form-data rather than JSON, because an optional .dem can ride along to have the score and map
		// computed from it — the file is parsed in memory and discarded, exactly like /api/stats/import-demo.
		group.MapPost("/", async (HttpRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			// CS2 demos routinely run 100-300MB — raise Kestrel's per-request cap (default 30MB) for this endpoint only.
			var sizeFeature = request.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
			if (sizeFeature is { IsReadOnly: false })
			{
				sizeFeature.MaxRequestBodySize = 320_000_000;
			}

			if (!request.HasFormContentType)
			{
				return Results.BadRequest("Oczekiwano danych formularza jako multipart/form-data.");
			}

			// ReadFormAsync's own MultipartBodyLengthLimit defaults to 128MB regardless of the Kestrel
			// request-body cap raised above — must be set separately or large demos are rejected mid-read.
			var formOptions = new FormOptions { MultipartBodyLengthLimit = 320_000_000 };
			var form = await request.ReadFormAsync(formOptions, cancellationToken);

			var (command, parseError) = BuildCommand(form);
			if (command is null)
			{
				return Results.BadRequest(parseError);
			}

			var file = form.Files.GetFile("demo");
			if (file is null || file.Length == 0)
			{
				var result = await sender.Send(command, cancellationToken);
				return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
			}

			await using var demoStream = file.OpenReadStream();
			var withDemo = await sender.Send(command with { DemoStream = demoStream }, cancellationToken);
			return withDemo.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
	}

	#endregion

	#region Private Methods

	/// <summary>Reads the manually entered form fields into an <see cref="AddResultCommand"/>, or reports the first unparseable one.</summary>
	private static (AddResultCommand? Command, string? ErrorMessage) BuildCommand(IFormCollection form)
	{
		var opponent = Text(form, "opponent") ?? string.Empty;

		if (!Enum.TryParse<MatchCategory>(Text(form, "category"), out var category))
		{
			return (null, "Nieprawidłowa kategoria rozgrywki.");
		}

		if (!DateTime.TryParse(
				Text(form, "playedAtUtc"),
				CultureInfo.InvariantCulture,
				DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
				out var playedAtUtc))
		{
			return (null, "Nieprawidłowa data meczu.");
		}

		if (!TryParseOptionalInt(Text(form, "ourScore"), out var ourScore))
		{
			return (null, "Nasz wynik musi być liczbą.");
		}

		if (!TryParseOptionalInt(Text(form, "opponentScore"), out var opponentScore))
		{
			return (null, "Wynik przeciwnika musi być liczbą.");
		}

		if (!TryParseOptionalGuid(Text(form, "tournamentId"), out var tournamentId))
		{
			return (null, "Nieprawidłowy identyfikator turnieju.");
		}

		if (!TryParseOptionalGuid(Text(form, "leagueId"), out var leagueId))
		{
			return (null, "Nieprawidłowy identyfikator ligi.");
		}

		return (new AddResultCommand(
			opponent,
			ourScore,
			opponentScore,
			Text(form, "mapName"),
			Text(form, "demoUrl"),
			Text(form, "notes"),
			playedAtUtc,
			category,
			tournamentId,
			leagueId), null);
	}

	/// <summary>Reads one form field, treating a blank value the same as an absent one.</summary>
	private static string? Text(IFormCollection form, string key)
	{
		var value = form[key].ToString();
		return string.IsNullOrWhiteSpace(value) ? null : value;
	}

	private static bool TryParseOptionalInt(string? value, out int? parsed)
	{
		parsed = null;
		if (value is null)
		{
			return true;
		}

		if (!int.TryParse(value, CultureInfo.InvariantCulture, out var number))
		{
			return false;
		}

		parsed = number;
		return true;
	}

	private static bool TryParseOptionalGuid(string? value, out Guid? parsed)
	{
		parsed = null;
		if (value is null)
		{
			return true;
		}

		if (!Guid.TryParse(value, out var guid))
		{
			return false;
		}

		parsed = guid;
		return true;
	}

	#endregion
}
