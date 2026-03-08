using FilamentBinding.iOS;

namespace Filament.Maui;

/// <summary>
/// iOS implementation of <see cref="IFilamentCamera"/>.
/// Wraps <see cref="FLTCamera"/> from the iOS binding.
/// On iOS the camera entity is managed internally by the FLT wrapper;
/// use <see cref="FLTCamera.EntityId"/> to retrieve the raw uint32 entity ID.
/// </summary>
internal sealed class FilamentCameraiOS : IFilamentCamera
{
    internal readonly FLTCamera _camera;

    internal FilamentCameraiOS(FLTCamera camera) =>
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));

    public void SetProjection(double fovDegrees, double aspect, double near, double far) =>
        _camera.SetProjection(fovDegrees, aspect, near, far);

    public void LookAt(
        double eyeX,    double eyeY,    double eyeZ,
        double centerX, double centerY, double centerZ,
        double upX,     double upY,     double upZ) =>
        _camera.LookAt(eyeX, eyeY, eyeZ, centerX, centerY, centerZ, upX, upY, upZ);

    /// <remarks>Destroyed via <see cref="IFilamentEngine.DestroyCamera"/>.</remarks>
    public void Dispose() { }
}
