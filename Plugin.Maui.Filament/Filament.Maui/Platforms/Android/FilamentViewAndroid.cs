using JFilament = Com.Google.Android.Filament;

namespace Filament.Maui;

/// <summary>
/// Android implementation of <see cref="IFilamentView"/>.
/// Wraps <see cref="JFilament.View"/> from the Java binding.
/// </summary>
internal sealed class FilamentViewAndroid : IFilamentView
{
    internal readonly JFilament.View _view;

    internal FilamentViewAndroid(JFilament.View view) =>
        _view = view ?? throw new ArgumentNullException(nameof(view));

    public void SetScene(IFilamentScene scene) =>
        _view.Scene = ((FilamentSceneAndroid)scene)._scene;

    public void SetCamera(IFilamentCamera camera) =>
        _view.Camera = ((FilamentCameraAndroid)camera)._camera;

    public void SetViewport(int left, int bottom, int width, int height) =>
        _view.Viewport = new JFilament.Viewport(left, bottom, width, height);

    public void SetPostProcessingEnabled(bool enabled) =>
        _view.PostProcessingEnabled = enabled;

    /// <remarks>Destroyed via <see cref="IFilamentEngine.DestroyView"/>.</remarks>
    public void Dispose() { }
}
