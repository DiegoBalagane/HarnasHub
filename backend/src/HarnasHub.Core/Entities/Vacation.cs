namespace HarnasHub.Core.Entities;

/// <summary>A player's time off covering a closed range of days, overriding daily availability.</summary>
public class Vacation
{
	#region Public Properties

	public Guid Id { get; set; }
	public Guid UserId { get; set; }
	public DateOnly StartDate { get; set; }
	public DateOnly EndDate { get; set; }
	public string? Reason { get; set; }
	public DateTime CreatedAtUtc { get; set; }

	#endregion
}
