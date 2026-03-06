# TASK-010: Cross-Platform Sample App

**Phase:** 4 — Integration and Validation
**Estimated Effort:** 3–5 days
**Depends On:** TASK-005, TASK-009
**Relevant Skills:** `filament-maui-api-surface`, `filament-surface-integration`, `filament-maui-project-structure`

## Objective

Build a minimal cross-platform MAUI sample application that demonstrates the complete Filament rendering pipeline on both Android and iOS using the `Filament.Maui` library. The sample should render a textured, lit 3D triangle or cube with a clear color background, proving end-to-end integration across both platforms with a single shared codebase.

## Prerequisites

- TASK-005 complete — Android `FilamentViewHandler` working
- TASK-009 complete — iOS `FilamentViewHandler` working
- TASK-003 complete — all cross-platform interfaces available
- Filament `matc` tool available (from the Filament release package) to compile `.matc` material source files
- A physical Android device or Android emulator with OpenGL ES 3.0 / Vulkan support
- A physical iOS device or Metal-capable Simulator

## Deliverables

- `maui/FilamentSample/FilamentSample.csproj` — MAUI application project targeting `net10.0-android;net10.0-ios`
- `maui/FilamentSample/MauiProgram.cs` — app setup with `UseFilament()` registration
- `maui/FilamentSample/MainPage.xaml` + `MainPage.xaml.cs` — page containing `FilamentView` control
- `maui/FilamentSample/Resources/Raw/materials/default.mat.android` — compiled Filament material for Android
- `maui/FilamentSample/Resources/Raw/materials/default.mat.ios` — compiled Filament material for iOS
- `maui/FilamentSample/Rendering/TriangleRenderer.cs` — shared rendering logic (vertex data, scene setup)
- Successful build and visible 3D output on both Android and iOS

## Detailed Steps

### Step 1: Create the MAUI application project

```bash
dotnet new maui -n FilamentSample -o maui/FilamentSample
```

Edit `maui/FilamentSample/FilamentSample.csproj` to add the `Filament.Maui` reference:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net10.0-android;net10.0-ios</TargetFrameworks>
    <UseMaui>true</UseMaui>
    <RootNamespace>FilamentSample</RootNamespace>
    <ApplicationId>com.example.filamentsample</ApplicationId>
    <ApplicationVersion>1</ApplicationVersion>
    <ApplicationDisplayVersion>1.0</ApplicationDisplayVersion>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Filament.Maui\Filament.Maui.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- Platform-compiled Filament materials -->
    <MauiAsset Include="Resources\Raw\materials\**" LogicalName="%(RecursiveDir)%(Filename)%(Extension)" />
  </ItemGroup>
</Project>
```

### Step 2: Register Filament in MauiProgram.cs

`maui/FilamentSample/MauiProgram.cs`:

```csharp
using Filament.Maui;
using Microsoft.Extensions.Logging;

namespace FilamentSample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseFilament()   // Registers FilamentViewHandler for both platforms
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
```

### Step 3: Add FilamentView to MainPage

`maui/FilamentSample/MainPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:filament="clr-namespace:Filament.Maui;assembly=Filament.Maui"
             x:Class="FilamentSample.MainPage"
             Title="Filament Sample">
    <Grid>
        <filament:FilamentView x:Name="FilamentSurface"
                               HorizontalOptions="Fill"
                               VerticalOptions="Fill"
                               FrameRendering="OnFrameRendering" />
        <Label Text="Filament .NET MAUI"
               TextColor="White"
               HorizontalOptions="Center"
               VerticalOptions="End"
               Margin="0,0,0,20" />
    </Grid>
</ContentPage>
```

### Step 4: Implement the code-behind

`maui/FilamentSample/MainPage.xaml.cs`:

```csharp
using Filament.Maui;

namespace FilamentSample;

public partial class MainPage : ContentPage
{
    private IFilamentEngine? _engine;
    private IFilamentScene? _scene;
    private IFilamentCamera? _camera;
    private IFilamentView? _filamentView;
    private TriangleRenderer? _triangle;

    public MainPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Engine creation must happen before binding to FilamentView.
        // For the initial sample, create on the main thread.
        // Production code should use a dedicated render thread.
        _engine = FilamentFactory.CreateEngine();
        _scene  = _engine.CreateScene();
        _camera = _engine.CreateCamera();

        _camera.SetProjection(60.0, 1.0, 0.1, 100.0);
        _camera.LookAt(0, 0, 3,  0, 0, 0,  0, 1, 0);

        // Load and set up geometry
        _triangle = new TriangleRenderer(_engine, _scene);

        FilamentSurface.Engine = _engine;
    }

    private void OnFrameRendering(object? sender, FilamentFrameEventArgs e)
    {
        _filamentView = e.View;
        e.View.SetScene(_scene!);
        e.View.SetCamera(_camera!);
        e.View.SetClearColor(0.15f, 0.15f, 0.2f, 1.0f);
        e.View.SetPostProcessingEnabled(false);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        FilamentSurface.Engine = null;

        if (_engine is not null)
        {
            _engine.FlushAndWait();
            _triangle?.Dispose(_engine);
            if (_filamentView != null) _engine.DestroyView(_filamentView);
            if (_scene != null)        _engine.DestroyScene(_scene);
            if (_camera != null)       _engine.DestroyCamera(_camera);
            _engine.Dispose();
            _engine = null;
        }
    }
}
```

### Step 5: Implement TriangleRenderer (shared geometry)

`maui/FilamentSample/Rendering/TriangleRenderer.cs`:

```csharp
using Filament.Maui;

namespace FilamentSample;

/// <summary>
/// Creates a single colored triangle entity in the Filament scene.
/// Uses platform-compiled materials loaded from app resources.
/// </summary>
internal sealed class TriangleRenderer
{
    private readonly int _entity;

    // Interleaved vertex data: position (xyz) + color (rgba u8)
    private static readonly float[] TriangleVertices =
    {
        //  X       Y      Z      R     G     B     A
         0.0f,  0.5f,  0.0f,  1.0f, 0.0f, 0.0f, 1.0f,
        -0.5f, -0.5f,  0.0f,  0.0f, 1.0f, 0.0f, 1.0f,
         0.5f, -0.5f,  0.0f,  0.0f, 0.0f, 1.0f, 1.0f,
    };

    private static readonly ushort[] TriangleIndices = { 0, 1, 2 };

    public TriangleRenderer(IFilamentEngine engine, IFilamentScene scene)
    {
        _entity = engine.EntityManager.Create();

        // Load material from platform-specific compiled asset
        var matBytes = LoadMaterialBytes();
        var material = FilamentMaterialLoader.LoadMaterial(engine, matBytes);
        var matInstance = material.CreateInstance();

        // Vertex/index buffer creation and renderable setup is platform-specific
        // at the Builder level. This simplified version sets material only.
        engine.RenderableManager.SetMaterialInstanceAt(_entity, 0, matInstance);

        scene.AddEntity(_entity);
    }

    public void Dispose(IFilamentEngine engine)
    {
        engine.EntityManager.Destroy(_entity);
    }

    private static byte[] LoadMaterialBytes()
    {
        // Load the platform-appropriate compiled material
#if ANDROID
        const string assetPath = "materials/default.mat.android";
#elif IOS
        const string assetPath = "materials/default.mat.ios";
#else
        throw new PlatformNotSupportedException();
#endif
        // Synchronously load from app package resources
        using var stream = FileSystem.OpenAppPackageFileAsync(assetPath).GetAwaiter().GetResult();
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
```

### Step 6: Compile platform materials

Using the `matc` compiler from the Filament release package:

Create `maui/FilamentSample/Materials/default.matc`:

```
material {
    name : "Default",
    shadingModel : unlit,
    parameters : []
}

vertex {
    void materialVertex(inout MaterialVertexInputs material) {}
}

fragment {
    void material(inout MaterialInputs material) {
        prepareMaterial(material);
        material.baseColor = getColor();
    }
}
```

Compile for each platform:
```bash
# Compile for Android (OpenGL ES + Vulkan)
matc -p mobile -a opengl -a vulkan \
     -o maui/FilamentSample/Resources/Raw/materials/default.mat.android \
     maui/FilamentSample/Materials/default.matc

# Compile for iOS (Metal)
matc -p mobile -a metal \
     -o maui/FilamentSample/Resources/Raw/materials/default.mat.ios \
     maui/FilamentSample/Materials/default.matc
```

`matc` is available in the Filament release package at:
```
https://github.com/google/filament/releases/download/v1.69.5/filament-v1.69.5-linux.tgz
```
(or macOS/Windows equivalents)

### Step 7: Build and run on each platform

```bash
# Build and run on Android
dotnet build maui/FilamentSample -f net10.0-android -c Debug
dotnet run --project maui/FilamentSample -f net10.0-android

# Build for iOS (requires macOS + Xcode)
dotnet build maui/FilamentSample -f net10.0-ios -c Debug
```

## Acceptance Criteria

- [ ] Sample app builds for both `net10.0-android` and `net10.0-ios` without errors
- [ ] `FilamentView` renders a visible colored background on Android (physical device or emulator)
- [ ] `FilamentView` renders a visible colored background on iOS (physical device or Metal simulator)
- [ ] No crashes on app startup, navigation away, or app backgrounding
- [ ] `OnDisappearing` correctly cleans up all Filament resources without memory leaks or GPU errors
- [ ] Material loading from `Resources/Raw/materials/` works on both platforms
- [ ] The shared `MainPage.xaml.cs` code contains no `#if ANDROID` / `#if IOS` guards (platform code is in `Platforms/` folders)
- [ ] `UseFilament()` in `MauiProgram.cs` is the only Filament-specific registration needed

## Reference

- See `.github/skills/filament-maui-api-surface/SKILL.md` — material pipeline and `#if` platform loading pattern
- See `.github/skills/filament-surface-integration/SKILL.md` — render thread rules and destroy order
- See `.github/skills/filament-maui-project-structure/SKILL.md` — overall solution layout
- See `docs/maui-binding-notes.md` — material pipeline and render thread management notes
- Filament iOS minimal sample: `ios/samples/HelloCocoaPods/HelloCocoaPods/ViewController.mm`
- Filament Android minimal sample: `android/samples/hello-triangle/`
- `matc` compiler: available in Filament release tarballs at `https://github.com/google/filament/releases`
