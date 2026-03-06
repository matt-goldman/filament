namespace Filament.Maui;

/// <summary>
/// Cross-platform MAUI <see cref="Microsoft.Maui.Controls.View"/> that hosts a
/// Filament rendering surface.
/// Platform handlers wire this to a <c>SurfaceView</c> on Android and a
/// <c>UIView+CAMetalLayer</c> on iOS via <c>FilamentViewHandler</c>.
/// </summary>
/// <remarks>
/// Register the handler in <c>MauiProgram.cs</c>:
/// <code>
/// builder.UseFilament();
/// </code>
/// </remarks>
public class FilamentView : Microsoft.Maui.Controls.View
{
    /// <summary>
    /// Bindable property backing <see cref="Engine"/>.
    /// Setting this property starts the render loop on the platform handler.
    /// </summary>
    public static readonly BindableProperty EngineProperty =
        BindableProperty.Create(
            nameof(Engine),
            typeof(IFilamentEngine),
            typeof(FilamentView));

    /// <summary>
    /// The Filament engine used to drive rendering for this view.
    /// Assign before or after the view is attached to a page;
    /// the handler will create the SwapChain as soon as both the engine and
    /// the native surface are ready.
    /// </summary>
    public IFilamentEngine? Engine
    {
        get => (IFilamentEngine?)GetValue(EngineProperty);
        set => SetValue(EngineProperty, value);
    }

    /// <summary>
    /// Raised by the platform handler each time a frame is about to be rendered.
    /// Subscribe to update scene content, camera, or material parameters.
    /// The event is raised on the render thread — do not call MAUI/UI APIs inside the handler.
    /// </summary>
    public event EventHandler<FilamentFrameEventArgs>? FrameRendering;

    /// <summary>
    /// Invoked by the platform handler to raise <see cref="FrameRendering"/>.
    /// </summary>
    internal void OnFrameRendering(FilamentFrameEventArgs e) =>
        FrameRendering?.Invoke(this, e);
}
