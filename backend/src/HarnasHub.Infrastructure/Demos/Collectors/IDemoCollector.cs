namespace HarnasHub.Infrastructure.Demos.Collectors;

/// <summary>One independent slice of what a demo pass extracts. Every enabled collector subscribes to the same
/// <c>CsDemoParser</c> before the single read, then writes its finished section into the shared builder — so adding
/// a new kind of data (economy, kills, position samples) means adding a collector, never editing an existing one.</summary>
internal interface IDemoCollector
{
	/// <summary>Hooks this collector's handlers onto the parser; called once, before the demo is read.</summary>
	void Subscribe();

	/// <summary>Writes everything gathered into <paramref name="builder"/>; called once, after the demo was read.</summary>
	void Contribute(DemoTimelineBuilder builder);
}
