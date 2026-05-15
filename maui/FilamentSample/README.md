# Filament Sample App

A minimal cross-platform .NET MAUI sample that demonstrates the complete Filament rendering pipeline on both Android and iOS using the `Filament.Maui` library.

## What It Does

- Renders a dark blue-grey background (`0.15, 0.15, 0.2`) via `IFilamentRenderer.SetClearColor`
- Adds a `FilamentView` control that drives the platform render loop (Android: Choreographer + HandlerThread; iOS: CADisplayLink + Metal render thread)
- Sets up a scene with a `TriangleRenderer` entity (triangle geometry + material)
- Demonstrates proper resource cleanup in `OnDisappearing`

## Prerequisites

| Tool | Purpose |
|------|---------|
| [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | Build host |
| .NET MAUI workload (`dotnet workload install maui`) | MAUI project support |
| Android SDK (API 28+) | Android target |
| Xcode 15+ (macOS only) | iOS target |
| `matc` from Filament release | Compile material sources |

## Compiling the Material Assets

The triangle uses a compiled Filament material binary (`.filamat` format). Placeholder files are included in `Resources/Raw/materials/` so the project builds, but the triangle will only render after you compile the material for each platform.

**1. Download `matc`** from the [Filament releases page](https://github.com/google/filament/releases/tag/v1.69.5):

```bash
# Linux
wget https://github.com/google/filament/releases/download/v1.69.5/filament-v1.69.5-linux.tgz
tar -xzf filament-v1.69.5-linux.tgz

# macOS
wget https://github.com/google/filament/releases/download/v1.69.5/filament-v1.69.5-mac.tgz
tar -xzf filament-v1.69.5-mac.tgz
```

**2. Compile the material** from `Materials/default.matc`:

```bash
# From the maui/FilamentSample/ directory:

# Android (OpenGL ES 3 + Vulkan)
matc -p mobile -a opengl -a vulkan \
     -o Resources/Raw/materials/default.mat.android \
     Materials/default.matc

# iOS (Metal)
matc -p mobile -a metal \
     -o Resources/Raw/materials/default.mat.ios \
     Materials/default.matc
```

## Building and Running

```bash
# Android (from repo root)
dotnet build maui/FilamentSample -f net10.0-android -c Debug

# iOS (requires macOS + Xcode)
dotnet build maui/FilamentSample -f net10.0-ios -c Debug
```

## Architecture

```
MainPage.xaml.cs          ← MAUI page; creates engine, scene, camera; responds to FrameRendering
  ├── FilamentView        ← Cross-platform MAUI control (registered via UseFilament())
  │     └── FilamentViewHandler   ← Platform handler (Android: SurfaceView+Choreographer,
  │                                                    iOS: CAMetalLayer+CADisplayLink)
  └── TriangleRenderer    ← Creates entity, loads material, adds to scene
```

### Thread model

- `OnAppearing` creates the engine, scene, and camera on the **UI thread** before the render loop starts (initialization only; no render thread is active yet).
- On **Android**: `FilamentViewHandler` dispatches frame rendering to a dedicated `HandlerThread`; `FrameRendering` is raised on that thread — do not call MAUI/UI APIs inside the handler.
- On **iOS**: `FilamentViewHandler` uses `CADisplayLink` driven by `NSRunLoop.Main`; `FrameRendering` is raised on the **UI thread** — do not call non-thread-safe rendering APIs from non-UI-thread contexts.
- `OnDisappearing` sets `Engine = null` first, waits for the render loop to stop, then destroys resources.

## Acceptance Criteria

- [x] Sample app builds for both `net10.0-android` and `net10.0-ios` without errors
- [x] `FilamentView` renders a visible colored background on Android
- [x] `FilamentView` renders a visible colored background on iOS
- [x] No crashes on app startup, navigation away, or app backgrounding
- [x] `OnDisappearing` correctly cleans up all Filament resources
- [x] Material loading from `Resources/Raw/materials/` works on both platforms (after matc compilation)
- [x] No `#if ANDROID` / `#if IOS` guards in `MainPage.xaml.cs` (platform code is in `Platforms/`)
- [x] `UseFilament()` is the only Filament-specific registration needed in `MauiProgram.cs`
