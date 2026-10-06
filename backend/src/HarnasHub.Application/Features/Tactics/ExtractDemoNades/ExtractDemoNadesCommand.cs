#region Usings

using ErrorOr;
using HarnasHub.Application.Features.Tactics.Shared;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.Tactics.ExtractDemoNades;

/// <summary>Parses a demo uploaded via the presigned demo upload and returns every round with its grenades, for the
/// import-tactic-from-demo wizard. The object is deleted afterwards either way. Coach/Manager only, enforced at the endpoint.</summary>
public record ExtractDemoNadesCommand(string ObjectKey) : IRequest<ErrorOr<DemoNadesDto>>;
