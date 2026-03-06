namespace Filament.Maui.Tests;

/// <summary>
/// Tests for <see cref="FilamentFrameEventArgs"/>.
/// Verifies constructor argument validation and property storage.
/// </summary>
public class FilamentFrameEventArgsTests
{
    [Fact]
    public void Constructor_StoresRendererProperty()
    {
        var renderer = new FakeRenderer();
        var view = new FakeView();

        var args = new FilamentFrameEventArgs(renderer, view);

        Assert.Same(renderer, args.Renderer);
    }

    [Fact]
    public void Constructor_StoresViewProperty()
    {
        var renderer = new FakeRenderer();
        var view = new FakeView();

        var args = new FilamentFrameEventArgs(renderer, view);

        Assert.Same(view, args.View);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenRendererIsNull()
    {
        var view = new FakeView();

        Assert.Throws<ArgumentNullException>(() =>
            new FilamentFrameEventArgs(null!, view));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenViewIsNull()
    {
        var renderer = new FakeRenderer();

        Assert.Throws<ArgumentNullException>(() =>
            new FilamentFrameEventArgs(renderer, null!));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenBothArgumentsAreNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new FilamentFrameEventArgs(null!, null!));
    }

    [Fact]
    public void RendererAndView_AreIndependentInstances()
    {
        var renderer1 = new FakeRenderer();
        var renderer2 = new FakeRenderer();
        var view = new FakeView();

        var args1 = new FilamentFrameEventArgs(renderer1, view);
        var args2 = new FilamentFrameEventArgs(renderer2, view);

        Assert.NotSame(args1.Renderer, args2.Renderer);
        Assert.Same(args1.View, args2.View);
    }
}
