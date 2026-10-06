#region Usings

using HarnasHub.Application.Features.Tactics.ExtractDemoNades;

#endregion

namespace HarnasHub.Tests.Application.Features.Tactics.ExtractDemoNades;

/// <summary>Covers the validation rules of <see cref="ExtractDemoNadesCommand"/>.</summary>
public class ExtractDemoNadesCommandValidatorTests
{
	#region Public Methods

	[Fact]
	public void Should_accept_an_object_key()
	{
		Assert.True(new ExtractDemoNadesCommandValidator().Validate(new ExtractDemoNadesCommand("demos/abc")).IsValid);
	}

	[Fact]
	public void Should_reject_an_empty_object_key()
	{
		Assert.False(new ExtractDemoNadesCommandValidator().Validate(new ExtractDemoNadesCommand("")).IsValid);
	}

	#endregion
}
