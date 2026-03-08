using FilamentBinding.iOS;

namespace Filament.Maui;

/// <summary>
/// iOS implementation of <see cref="IFilamentView"/>.
/// Wraps <see cref="FLTView"/> from the iOS binding.
/// </summary>
internal sealed class FilamentViewiOS : IFilamentView
{
    internal readonly FLTView _view;

    internal FilamentViewiOS(FLTView view) =>
        _view = view ?? throw new ArgumentNullException(nameof(view));

    public void SetScene(IFilamentScene scene) =>
        _view.SetScene(((FilamentSceneiOS)scene)._scene);

    public void SetCamera(IFilamentCamera camera) =>
        _view.SetCamera(((FilamentCameraiOS)camera)._camera);

    public void SetViewport(int left, int bottom, int width, int height)
    {
        if (width < 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Viewport width must be non-negative.");

        if (height < 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Viewport height must be non-negative.");

        _view.SetViewport(left, bottom, (uint)width, (uint)height);
    }

    public void SetPostProcessingEnabled(bool enabled) =>
        _view.SetPostProcessingEnabled(enabled);

    /// <remarks>Destroyed via <see cref="IFilamentEngine.DestroyView"/>.</remarks>
    public void Dispose() { }
}
