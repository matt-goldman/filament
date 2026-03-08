namespace Filament.Maui;

/// <summary>
/// Perspective or orthographic camera. Controls projection matrix and view (look-at) matrix.
/// </summary>
public interface IFilamentCamera : IDisposable
{
    /// <summary>
    /// Sets the perspective projection using a vertical field-of-view.
    /// </summary>
    /// <param name="fovDegrees">Vertical field of view in degrees.</param>
    /// <param name="aspect">Aspect ratio (width / height).</param>
    /// <param name="near">Near clipping plane distance (must be &gt; 0).</param>
    /// <param name="far">Far clipping plane distance (must be &gt; near).</param>
    void SetProjection(double fovDegrees, double aspect, double near, double far);

    /// <summary>
    /// Sets the camera position and orientation using a look-at specification.
    /// All coordinates are in world space.
    /// </summary>
    void LookAt(
        double eyeX,    double eyeY,    double eyeZ,
        double centerX, double centerY, double centerZ,
        double upX,     double upY,     double upZ);
}
