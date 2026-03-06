namespace Filament.Maui.Tests;

// ---------------------------------------------------------------------------
// Fake implementations of Filament interfaces for use in unit tests.
// These stubs record calls and return predictable values so tests can verify
// interaction patterns without requiring native Filament libraries.
// ---------------------------------------------------------------------------

internal sealed class FakeEngine : IFilamentEngine
{
    public IFilamentRenderer CreateRenderer() => new FakeRenderer();
    public IFilamentScene CreateScene() => new FakeScene();
    public IFilamentView CreateView() => new FakeView();
    public IFilamentCamera CreateCamera() => new FakeCamera();
    public IFilamentSwapChain CreateSwapChain(object nativeSurface) => new FakeSwapChain();
    public IFilamentEntityManager EntityManager => new FakeEntityManager();
    public IFilamentTransformManager TransformManager => new FakeTransformManager();
    public IFilamentRenderableManager RenderableManager => new FakeRenderableManager();
    public void DestroyRenderer(IFilamentRenderer renderer) { }
    public void DestroyScene(IFilamentScene scene) { }
    public void DestroyView(IFilamentView view) { }
    public void DestroyCamera(IFilamentCamera camera) { }
    public void DestroySwapChain(IFilamentSwapChain swapChain) { }
    public void FlushAndWait() { }
    public void Dispose() { }
}

internal sealed class FakeRenderer : IFilamentRenderer
{
    public bool RenderCalled { get; private set; }
    public bool EndFrameCalled { get; private set; }

    public bool BeginFrame(IFilamentSwapChain swapChain) => true;
    public void Render(IFilamentView view) => RenderCalled = true;
    public void EndFrame() => EndFrameCalled = true;
    public void SetClearColor(float r, float g, float b, float a) { }
    public void Dispose() { }
}

internal sealed class FakeView : IFilamentView
{
    public void SetScene(IFilamentScene scene) { }
    public void SetCamera(IFilamentCamera camera) { }
    public void SetViewport(int left, int bottom, int width, int height) { }
    public void SetPostProcessingEnabled(bool enabled) { }
    public void Dispose() { }
}

internal sealed class FakeScene : IFilamentScene
{
    public void AddEntity(int entity) { }
    public void RemoveEntity(int entity) { }
    public void SetSkybox(IFilamentSkybox? skybox) { }
    public void SetIndirectLight(IFilamentIndirectLight? ibl) { }
    public void Dispose() { }
}

internal sealed class FakeCamera : IFilamentCamera
{
    public void SetProjection(double fovDegrees, double aspect, double near, double far) { }
    public void LookAt(double eyeX, double eyeY, double eyeZ,
                       double centerX, double centerY, double centerZ,
                       double upX, double upY, double upZ) { }
    public void Dispose() { }
}

internal sealed class FakeSwapChain : IFilamentSwapChain
{
    public void Dispose() { }
}

internal sealed class FakeEntityManager : IFilamentEntityManager
{
    private int _next = 1;

    public int Create() => _next++;
    public void Destroy(int entity) { }
}

internal sealed class FakeTransformManager : IFilamentTransformManager
{
    public void Create(int entity) { }
    public void SetTransform(int entity, float[] mat4ColumnMajor) { }
}

internal sealed class FakeRenderableManager : IFilamentRenderableManager
{
    public void SetMaterialInstanceAt(int entity, int primitiveIndex, IFilamentMaterialInstance instance) { }
}

internal sealed class FakeMaterial : IFilamentMaterial
{
    public IFilamentMaterialInstance CreateInstance() => new FakeMaterialInstance();
    public void Dispose() { }
}

internal sealed class FakeMaterialInstance : IFilamentMaterialInstance
{
    public void SetParameterFloat(string name, float value) { }
    public void SetParameterFloat4(string name, float x, float y, float z, float w) { }
    public void SetParameterTexture(string name, IFilamentTexture texture) { }
    public void Dispose() { }
}

internal sealed class FakeTexture : IFilamentTexture
{
    public void Dispose() { }
}
