namespace Filament.Maui;

/// <summary>
/// Executes a Filament frame: begin → render → end.
/// Must be driven from a dedicated render thread; <see cref="BeginFrame"/> and
/// <see cref="EndFrame"/> are not thread-safe relative to the engine.
/// </summary>
public interface IFilamentRenderer : IDisposable
{
    /// <summary>
    /// Starts a new frame. Returns <see langword="true"/> if the frame should proceed
    /// (surface is valid and not throttled).
    /// </summary>
    bool BeginFrame(IFilamentSwapChain swapChain);

    /// <summary>Renders the specified view into the current frame.</summary>
    void Render(IFilamentView view);

    /// <summary>Finalizes and presents the current frame.</summary>
    void EndFrame();

    /// <summary>
    /// Sets the background clear color for all rendered views.
    /// Corresponds to <c>Renderer.ClearOptions.clearColor</c> on Android and
    /// <c>FLTRenderer</c> clear color on iOS.
    /// Call before <see cref="BeginFrame"/> for the change to take effect.
    /// </summary>
    void SetClearColor(float r, float g, float b, float a);
}
