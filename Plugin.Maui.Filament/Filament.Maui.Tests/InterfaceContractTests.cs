namespace Filament.Maui.Tests;

/// <summary>
/// Tests verifying that all core interfaces define the expected members.
/// These tests catch accidental removal of interface members that would
/// constitute a breaking change in the public API contract.
/// </summary>
public class InterfaceContractTests
{
    // IFilamentEngine

    [Fact]
    public void IFilamentEngine_ImplementableByFakeEngine()
    {
        // Verifies that IFilamentEngine can be implemented with the minimal expected API.
        IFilamentEngine engine = new FakeEngine();

        Assert.NotNull(engine.CreateRenderer());
        Assert.NotNull(engine.CreateScene());
        Assert.NotNull(engine.CreateView());
        Assert.NotNull(engine.CreateCamera());
        Assert.NotNull(engine.EntityManager);
        Assert.NotNull(engine.TransformManager);
        Assert.NotNull(engine.RenderableManager);
    }

    [Fact]
    public void IFilamentEngine_CreateSwapChain_ReturnsNonNull()
    {
        IFilamentEngine engine = new FakeEngine();
        var swapChain = engine.CreateSwapChain(new object());
        Assert.NotNull(swapChain);
    }

    [Fact]
    public void IFilamentEngine_FlushAndWait_DoesNotThrow()
    {
        IFilamentEngine engine = new FakeEngine();
        // FlushAndWait must be callable without throwing when no GPU work is pending.
        engine.FlushAndWait();
    }

    // IFilamentRenderer

    [Fact]
    public void IFilamentRenderer_RenderCycle_CallsInOrder()
    {
        var renderer = new FakeRenderer();
        var swapChain = new FakeSwapChain();
        var view = new FakeView();

        bool began = renderer.BeginFrame(swapChain);
        renderer.Render(view);
        renderer.EndFrame();

        Assert.True(began);
        Assert.True(renderer.RenderCalled);
        Assert.True(renderer.EndFrameCalled);
    }

    // IFilamentEntityManager

    [Fact]
    public void IFilamentEntityManager_Create_ReturnsUniqueIds()
    {
        IFilamentEntityManager mgr = new FakeEntityManager();
        var id1 = mgr.Create();
        var id2 = mgr.Create();

        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void IFilamentEntityManager_Destroy_DoesNotThrow()
    {
        IFilamentEntityManager mgr = new FakeEntityManager();
        var id = mgr.Create();
        mgr.Destroy(id); // Should not throw
    }

    // IFilamentTransformManager

    [Fact]
    public void IFilamentTransformManager_SetTransform_AcceptsIdentityMatrix()
    {
        IFilamentTransformManager mgr = new FakeTransformManager();
        var identity = new float[]
        {
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1,
        };
        mgr.Create(42);
        mgr.SetTransform(42, identity); // Should not throw for a valid 16-element matrix
    }

    // IFilamentView

    [Fact]
    public void IFilamentView_SetViewport_AcceptsZeroOrigin()
    {
        IFilamentView view = new FakeView();
        view.SetViewport(0, 0, 1920, 1080); // Common resolution — should not throw
    }

    [Fact]
    public void IFilamentRenderer_SetClearColor_AcceptsValidNormalizedValues()
    {
        IFilamentRenderer renderer = new FakeRenderer();
        renderer.SetClearColor(0.2f, 0.4f, 0.8f, 1.0f); // Blue background — should not throw
    }

    // IFilamentScene

    [Fact]
    public void IFilamentScene_AddAndRemoveEntity_RoundTrips()
    {
        IFilamentScene scene = new FakeScene();
        scene.AddEntity(1);
        scene.RemoveEntity(1); // Should not throw
    }

    [Fact]
    public void IFilamentScene_SetSkyboxNull_DoesNotThrow()
    {
        IFilamentScene scene = new FakeScene();
        scene.SetSkybox(null); // Clearing the skybox must be supported
    }

    [Fact]
    public void IFilamentScene_SetIndirectLightNull_DoesNotThrow()
    {
        IFilamentScene scene = new FakeScene();
        scene.SetIndirectLight(null); // Clearing IBL must be supported
    }

    // IFilamentCamera

    [Fact]
    public void IFilamentCamera_SetProjection_DoesNotThrow()
    {
        IFilamentCamera camera = new FakeCamera();
        camera.SetProjection(60.0, 16.0 / 9.0, 0.1, 1000.0);
    }

    [Fact]
    public void IFilamentCamera_LookAt_DoesNotThrow()
    {
        IFilamentCamera camera = new FakeCamera();
        camera.LookAt(0, 0, 5, 0, 0, 0, 0, 1, 0);
    }
}
