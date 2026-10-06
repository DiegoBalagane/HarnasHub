namespace HarnasHub.Infrastructure.Demos;

/// <summary>Read-only pass-through stream reporting how much of a seekable inner stream has been read (0–1). Progress counts
/// bytes actually read, not the position, so an occasional seek (e.g. to a trailer) can't make it jump to the end. Reports
/// at most every half percent. Never disposes the inner stream — its owner does.</summary>
public sealed class ProgressReportingStream(Stream inner, Action<double> onProgress) : Stream
{
	#region Private Fields

	private const int ReportStepPermille = 5;

	private long _bytesRead;
	private int _lastPermille = -ReportStepPermille;

	#endregion

	#region Public Properties

	/// <inheritdoc />
	public override bool CanRead => inner.CanRead;

	/// <inheritdoc />
	public override bool CanSeek => inner.CanSeek;

	/// <inheritdoc />
	public override bool CanWrite => false;

	/// <inheritdoc />
	public override long Length => inner.Length;

	/// <inheritdoc />
	public override long Position
	{
		get => inner.Position;
		set => inner.Position = value;
	}

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public override int Read(byte[] buffer, int offset, int count) => Advance(inner.Read(buffer, offset, count));

	/// <inheritdoc />
	public override int Read(Span<byte> buffer) => Advance(inner.Read(buffer));

	/// <inheritdoc />
	public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
		Advance(await inner.ReadAsync(buffer.AsMemory(offset, count), cancellationToken));

	/// <inheritdoc />
	public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
		Advance(await inner.ReadAsync(buffer, cancellationToken));

	/// <inheritdoc />
	public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

	/// <inheritdoc />
	public override void Flush()
	{
	}

	/// <inheritdoc />
	public override void SetLength(long value) => throw new NotSupportedException();

	/// <inheritdoc />
	public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

	#endregion

	#region Private Methods

	private int Advance(int read)
	{
		if (read <= 0 || !inner.CanSeek)
		{
			return read;
		}

		_bytesRead += read;
		var length = inner.Length;
		if (length <= 0)
		{
			return read;
		}

		var permille = (int)Math.Min(1000, _bytesRead * 1000 / length);
		if (permille >= _lastPermille + ReportStepPermille || (permille == 1000 && _lastPermille < 1000))
		{
			_lastPermille = permille;
			onProgress(permille / 1000d);
		}

		return read;
	}

	#endregion
}
