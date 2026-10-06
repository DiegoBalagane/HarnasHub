#region Usings

using ErrorOr;
using HarnasHub.Application.Features.Jobs.Shared;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.Jobs.StartJob;

/// <summary>Validates <paramref name="Request"/> (a request registered in <see cref="JobRegistry"/>), stores it as a queued
/// background job of the current user and hands it to the worker. Authorization of the operation itself stays on the
/// endpoint that sends this.</summary>
public record StartJobCommand(IBaseRequest Request) : IRequest<ErrorOr<JobAcceptedDto>>;
