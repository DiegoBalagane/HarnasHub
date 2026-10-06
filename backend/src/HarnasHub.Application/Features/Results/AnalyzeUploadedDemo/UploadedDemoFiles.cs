#region Usings

using System.Text.RegularExpressions;

#endregion

namespace HarnasHub.Application.Features.Results.AnalyzeUploadedDemo;

/// <summary>Naming of the temp files a direct demo upload is buffered into — the job payload only ever points at a file
/// of this exact shape in the temp directory, so a payload can never make the server read (and delete) anything else.</summary>
public static partial class UploadedDemoFiles
{
	#region Public Methods

	/// <summary>A fresh temp file path for one uploaded demo.</summary>
	public static string NewTempPath() => Path.Combine(Path.GetTempPath(), $"harnashub-demo-{Guid.NewGuid():N}.dem");

	/// <summary>Whether <paramref name="path"/> is a path produced by <see cref="NewTempPath"/>.</summary>
	public static bool IsUploadTempPath(string? path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return false;
		}

		var directory = Path.GetDirectoryName(path);
		var tempDirectory = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		return string.Equals(directory, tempDirectory, StringComparison.Ordinal) && TempFileName().IsMatch(Path.GetFileName(path));
	}

	/// <summary>Deletes the temp file, ignoring failures (a stray temp file in a recycled container is harmless).</summary>
	public static void TryDelete(string path)
	{
		try
		{
			File.Delete(path);
		}
		catch (Exception)
		{
			// Best-effort.
		}
	}

	#endregion

	#region Private Methods

	[GeneratedRegex("^harnashub-demo-[0-9a-f]{32}\\.dem$")]
	private static partial Regex TempFileName();

	#endregion
}
