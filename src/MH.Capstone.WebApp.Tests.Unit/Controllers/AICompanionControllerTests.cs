using System.Diagnostics.CodeAnalysis;
using MH.Capstone.Domain.Services.Abstraction;
using MH.Capstone.WebApp.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace MH.Capstone.WebApp.Tests.Unit.Controllers;

[TestFixture]
[ExcludeFromCodeCoverage]
public class AICompanionControllerTests
{
    [Test]
    public async Task Ask_WithAiService_ReturnsReply()
    {
        var ai = new Mock<IAIService>();
        ai.Setup(a => a.AskAsync("What do owls eat?", It.IsAny<CancellationToken>())).ReturnsAsync("Mice, mostly.");
        using var controller = new AICompanionController(NullLogger<AICompanionController>.Instance, ai.Object);

        var result = await controller.Ask("What do owls eat?");

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task Ask_WhenAiServiceDisabled_Returns503()
    {
        using var controller = new AICompanionController(NullLogger<AICompanionController>.Instance);

        var result = await controller.Ask("What do owls eat?");

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(503));
    }

    // MVC builds controllers with ActivatorUtilities. With the Gemini flag off, IAIService isn't
    // registered at all; this is the exact failure seen in production before the optional parameter.
    [TestCase(typeof(AICompanionController))]
    [TestCase(typeof(SightingAIController))]
    public void Controller_CanBeActivated_WhenNoAiServiceIsRegistered(Type controllerType)
    {
        using var provider = new ServiceCollection()
            .AddLogging()
            .AddSingleton(Mock.Of<ISightingsService>())
            .BuildServiceProvider();

        var factory = ActivatorUtilities.CreateFactory(controllerType, Type.EmptyTypes);

        Assert.That(() => factory(provider, null), Throws.Nothing);
    }
}
