using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.Results.PresignDemoUpload;

/// <summary>Requests a time-limited URL the browser can upload a demo file to directly — bypassing this app's own
/// HTTP pipeline entirely, for demos too large for the hosting platform's own request-size limit. Coach/Manager
/// only, enforced at the endpoint.</summary>
public record PresignDemoUploadCommand : IRequest<ErrorOr<PresignDemoUploadResultDto>>;

/// <summary><paramref name="ObjectKey"/> is opaque to the client — it's only ever passed back verbatim to
/// <c>AnalyzeDemoFromStorageCommand</c> once the upload to <paramref name="UploadUrl"/> finishes.</summary>
public record PresignDemoUploadResultDto(string UploadUrl, string ObjectKey);
