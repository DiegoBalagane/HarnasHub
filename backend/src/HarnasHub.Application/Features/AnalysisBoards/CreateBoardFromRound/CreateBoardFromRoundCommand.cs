#region Usings

using ErrorOr;
using HarnasHub.Application.Features.AnalysisBoards.Shared;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.AnalysisBoards.CreateBoardFromRound;

/// <summary>Which stored timeline a replayed round comes from.</summary>
public enum ReplaySource
{
	Match = 0,
	OpponentDemo = 1
}

/// <summary>Saves the replayed round <paramref name="RoundNumber"/> at <paramref name="Second"/> (from freeze end) as a new
/// analysis board on its map: player positions, grenades, deaths and the bomb drawn as strokes. Coach/Manager only.
/// <paramref name="SourceId"/> is the match result id or the opponent demo id, depending on <paramref name="Source"/>.</summary>
public record CreateBoardFromRoundCommand(ReplaySource Source, Guid SourceId, int RoundNumber, int Second)
	: IRequest<ErrorOr<AnalysisBoardDto>>;
