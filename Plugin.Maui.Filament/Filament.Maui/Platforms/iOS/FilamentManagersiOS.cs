using FilamentBinding.iOS;

namespace Filament.Maui;

/// <summary>
/// iOS implementation of <see cref="IFilamentEntityManager"/>.
/// Wraps <see cref="FLTEntityManager"/> from the iOS binding.
/// Entity IDs are <c>uint</c> in the FLT* API and <c>int</c> in the cross-platform interface.
/// </summary>
internal sealed class FilamentEntityManageriOS : IFilamentEntityManager
{
    private readonly FLTEntityManager _mgr;

    internal FilamentEntityManageriOS(FLTEntityManager mgr) =>
        _mgr = mgr ?? throw new ArgumentNullException(nameof(mgr));

    public int Create()
    {
        uint rawId = _mgr.Create();
        if (rawId > (uint)int.MaxValue)
            throw new OverflowException(
                $"FLTEntityManager returned an entity ID ({rawId}) that exceeds int.MaxValue and cannot be represented in the cross-platform interface.");
        return (int)rawId;
    }

    public void Destroy(int entity)
    {
        if (entity < 0)
            throw new ArgumentOutOfRangeException(nameof(entity), "Entity ID must be non-negative.");
        _mgr.Destroy((uint)entity);
    }
}

/// <summary>
/// iOS implementation of <see cref="IFilamentTransformManager"/>.
/// Wraps <see cref="FLTTransformManager"/> from the iOS binding.
/// </summary>
internal sealed class FilamentTransformManageriOS : IFilamentTransformManager
{
    private readonly FLTTransformManager _mgr;

    internal FilamentTransformManageriOS(FLTTransformManager mgr) =>
        _mgr = mgr ?? throw new ArgumentNullException(nameof(mgr));

    public void Create(int entity)
    {
        if (entity < 0)
            throw new ArgumentOutOfRangeException(nameof(entity), "Entity ID must be non-negative.");

        _mgr.CreateComponent((uint)entity);
    }

    public void SetTransform(int entity, float[] mat4ColumnMajor)
    {
        if (entity < 0)
            throw new ArgumentOutOfRangeException(nameof(entity), "Entity ID must be non-negative.");
        if (mat4ColumnMajor is null) throw new ArgumentNullException(nameof(mat4ColumnMajor));
        if (mat4ColumnMajor.Length != 16)
            throw new ArgumentException(
                "Transform matrix must have exactly 16 elements (4×4 column-major).",
                nameof(mat4ColumnMajor));

        // Convert float[16] column-major array to the blittable MatrixFloat4x4 struct
        // whose memory layout matches simd_float4x4 on arm64.
        var m = new MatrixFloat4x4
        {
            // Column 0
            C0R0 = mat4ColumnMajor[0],  C0R1 = mat4ColumnMajor[1],
            C0R2 = mat4ColumnMajor[2],  C0R3 = mat4ColumnMajor[3],
            // Column 1
            C1R0 = mat4ColumnMajor[4],  C1R1 = mat4ColumnMajor[5],
            C1R2 = mat4ColumnMajor[6],  C1R3 = mat4ColumnMajor[7],
            // Column 2
            C2R0 = mat4ColumnMajor[8],  C2R1 = mat4ColumnMajor[9],
            C2R2 = mat4ColumnMajor[10], C2R3 = mat4ColumnMajor[11],
            // Column 3
            C3R0 = mat4ColumnMajor[12], C3R1 = mat4ColumnMajor[13],
            C3R2 = mat4ColumnMajor[14], C3R3 = mat4ColumnMajor[15],
        };
        _mgr.SetTransform(m, (uint)entity);
    }
}

/// <summary>
/// iOS implementation of <see cref="IFilamentRenderableManager"/>.
/// Wraps <see cref="FLTRenderableManager"/> from the iOS binding.
/// </summary>
internal sealed class FilamentRenderableManageriOS : IFilamentRenderableManager
{
    private readonly FLTRenderableManager _mgr;

    internal FilamentRenderableManageriOS(FLTRenderableManager mgr) =>
        _mgr = mgr ?? throw new ArgumentNullException(nameof(mgr));

    public void SetMaterialInstanceAt(int entity, int primitiveIndex, IFilamentMaterialInstance instance)
    {
        // The iOS FLT binding does not currently expose a runtime SetMaterialInstanceAt
        // method on FLTRenderableManager. Materials are assigned at construction time via
        // FLTRenderableManagerBuilder.MaterialAtIndex(). Setting materials at runtime on iOS
        // requires rebuilding the renderable component.
        throw new NotSupportedException(
            "SetMaterialInstanceAt is not supported on iOS via the current FLT binding. " +
            "Assign materials at renderable construction time using FLTRenderableManagerBuilder.MaterialAtIndex().");
    }
}
