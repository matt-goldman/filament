using JFilament = Com.Google.Android.Filament;

namespace Filament.Maui;

/// <summary>
/// Android implementation of <see cref="IFilamentEntityManager"/>.
/// Wraps the singleton <see cref="JFilament.EntityManager"/> from the Java binding.
/// </summary>
internal sealed class FilamentEntityManagerAndroid : IFilamentEntityManager
{
    private readonly JFilament.EntityManager _mgr;

    internal FilamentEntityManagerAndroid(JFilament.EntityManager mgr) =>
        _mgr = mgr ?? throw new ArgumentNullException(nameof(mgr));

    public int Create() => _mgr.Create();

    public void Destroy(int entity) => _mgr.Destroy(entity);
}

/// <summary>
/// Android implementation of <see cref="IFilamentTransformManager"/>.
/// Wraps <see cref="JFilament.TransformManager"/> from the Java binding.
/// </summary>
internal sealed class FilamentTransformManagerAndroid : IFilamentTransformManager
{
    private readonly JFilament.TransformManager _mgr;

    internal FilamentTransformManagerAndroid(JFilament.TransformManager mgr) =>
        _mgr = mgr ?? throw new ArgumentNullException(nameof(mgr));

    public void Create(int entity) => _mgr.Create(entity);

    public void SetTransform(int entity, float[] mat4ColumnMajor)
    {
        if (mat4ColumnMajor is null) throw new ArgumentNullException(nameof(mat4ColumnMajor));
        if (mat4ColumnMajor.Length != 16)
            throw new ArgumentException("Transform matrix must have exactly 16 elements (4x4 column-major).", nameof(mat4ColumnMajor));

        _mgr.SetTransform(entity, mat4ColumnMajor);
    }
}

/// <summary>
/// Android implementation of <see cref="IFilamentRenderableManager"/>.
/// Wraps <see cref="JFilament.RenderableManager"/> from the Java binding.
/// </summary>
internal sealed class FilamentRenderableManagerAndroid : IFilamentRenderableManager
{
    private readonly JFilament.RenderableManager _mgr;

    internal FilamentRenderableManagerAndroid(JFilament.RenderableManager mgr) =>
        _mgr = mgr ?? throw new ArgumentNullException(nameof(mgr));

    public void SetMaterialInstanceAt(int entity, int primitiveIndex, IFilamentMaterialInstance instance)
    {
        var componentInstance = _mgr.GetInstance(entity);
        _mgr.SetMaterialInstanceAt(
            componentInstance,
            primitiveIndex,
            ((FilamentMaterialInstanceAndroid)instance)._instance);
    }
}
