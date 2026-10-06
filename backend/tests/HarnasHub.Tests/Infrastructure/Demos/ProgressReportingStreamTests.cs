#region Usings

using HarnasHub.Infrastructure.Demos;
using Xunit;

#endregion

namespace HarnasHub.Tests.Infrastructure.Demos;

public class ProgressReportingStreamTests
{
	#region Public Methods

	[Fact]
	public async Task Should_report_the_fraction_of_bytes_read_up_to_one()
	{
		var inner = new MemoryStream(new byte[1000]);
		var reports = new List<double>();
		await using var stream = new ProgressReportingStream(inner, reports.Add);

		var buffer = new byte[100];
		while (await stream.ReadAsync(buffer) > 0)
		{
		}

		Assert.Equal(1.0, reports[^1]);
		Assert.Equal(reports.OrderBy(r => r), reports);
		Assert.Equal(10, reports.Count);
	}

	[Fact]
	public void Should_count_bytes_read_not_position_so_a_seek_to_the_end_does_not_jump_to_done()
	{
		var inner = new MemoryStream(new byte[1000]);
		var reports = new List<double>();
		using var stream = new ProgressReportingStream(inner, reports.Add);

		stream.Seek(-10, SeekOrigin.End);
		_ = stream.Read(new byte[10], 0, 10);
		stream.Position = 0;
		_ = stream.Read(new byte[200], 0, 200);

		Assert.Equal(0.21, reports[^1], 3);
	}

	[Fact]
	public void Should_skip_reports_smaller_than_half_a_percent()
	{
		var inner = new MemoryStream(new byte[10_000]);
		var reports = new List<double>();
		using var stream = new ProgressReportingStream(inner, reports.Add);

		for (var i = 0; i < 100; i++)
		{
			_ = stream.Read(new byte[10], 0, 10);
		}

		Assert.Equal(20, reports.Count);
	}

	[Fact]
	public void Should_not_dispose_the_inner_stream()
	{
		var inner = new MemoryStream(new byte[10]);

		new ProgressReportingStream(inner, _ => { }).Dispose();

		Assert.True(inner.CanRead);
	}

	#endregion
}
