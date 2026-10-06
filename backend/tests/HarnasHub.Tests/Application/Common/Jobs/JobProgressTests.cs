#region Usings

using HarnasHub.Application.Common.Jobs;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Common.Jobs;

public class JobProgressTests
{
	#region Public Methods

	[Fact]
	public void Should_do_nothing_outside_a_job()
	{
		var progress = new JobProgress(new JobExecutionContext());

		progress.Report(50, "Analiza demki");
		progress.ReportStep(0.5);
	}

	[Fact]
	public void Should_map_step_fractions_into_the_default_demo_window_and_switch_to_saving_at_the_end()
	{
		var (progress, sink) = Create();

		progress.ReportStep(0);
		progress.ReportStep(0.5);
		progress.ReportStep(1);

		Assert.Equal(
			[(5, JobProgress.DefaultStepStage), (50, JobProgress.DefaultStepStage), (95, JobProgress.SavingStage)],
			sink.Published);
	}

	[Fact]
	public void Should_map_into_an_explicit_step_and_keep_its_label_at_the_end()
	{
		var (progress, sink) = Create();

		progress.BeginStep(20, 40, "Demka 1 z 2: analiza");
		progress.ReportStep(0.5);
		progress.ReportStep(1);

		Assert.Equal(
			[(20, "Demka 1 z 2: analiza"), (30, "Demka 1 z 2: analiza"), (40, "Demka 1 z 2: analiza")],
			sink.Published);
	}

	[Fact]
	public void Should_never_move_the_bar_backwards_but_still_update_the_stage()
	{
		var (progress, sink) = Create();

		progress.Report(60, "Pobieranie historii meczów");
		progress.Report(10, "Budowanie raportu");
		progress.Report(150);

		Assert.Equal([(60, "Pobieranie historii meczów"), (60, "Budowanie raportu"), (100, "Budowanie raportu")], sink.Published);
	}

	#endregion

	#region Private Methods

	private static (JobProgress Progress, RecordingSink Sink) Create()
	{
		var sink = new RecordingSink();
		var context = new JobExecutionContext();
		context.Begin(Guid.NewGuid(), Guid.NewGuid(), sink);
		return (new JobProgress(context), sink);
	}

	#endregion

	#region Nested Types

	private sealed class RecordingSink : IJobProgressSink
	{
		public List<(int Percent, string? Stage)> Published { get; } = [];

		public void Publish(int percent, string? stage) => Published.Add((percent, stage));
	}

	#endregion
}
