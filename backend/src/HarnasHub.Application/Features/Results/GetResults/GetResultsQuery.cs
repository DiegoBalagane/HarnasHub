using ErrorOr;
using HarnasHub.Application.Features.Results.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Results.GetResults;

/// <summary>Returns every logged result, most recent first.</summary>
public record GetResultsQuery : IRequest<ErrorOr<List<MatchResultDto>>>;
