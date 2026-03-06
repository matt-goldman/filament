using JFilament = Com.Google.Android.Filament;

namespace Filament.Maui;

/// <summary>
/// Android implementation of <see cref="IFilamentCamera"/>.
/// Wraps <see cref="JFilament.Camera"/> from the Java binding.
/// </summary>
internal sealed class FilamentCameraAndroid : IFilamentCamera
{
    internal readonly JFilament.Camera _camera;
    internal readonly int _entityId;
    private readonly FilamentEngineAndroid _engine;

    internal FilamentCameraAndroid(JFilament.Camera camera, int entityId, FilamentEngineAndroid engine)
    {
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        _entityId = entityId;
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    public void SetProjection(double fovDegrees, double aspect, double near, double far) =>
        _camera.SetProjection(fovDegrees, aspect, near, far, JFilament.Camera.Fov.Vertical);

    public void LookAt(
        double eyeX,    double eyeY,    double eyeZ,
        double centerX, double centerY, double centerZ,
        double upX,     double upY,     double upZ) =>
        _camera.LookAt(eyeX, eyeY, eyeZ, centerX, centerY, centerZ, upX, upY, upZ);

    /// <remarks>Destroyed via <see cref="IFilamentEngine.DestroyCamera"/>.</remarks>
    public void Dispose() { }
}
