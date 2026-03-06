---
name: filament-ios-binding
description: >
  Guidance for creating a .NET MAUI iOS binding library for Filament.
  Covers the critical C++-only API blocker, required Objective-C++ wrapper strategy,
  wrapper class design, XCFramework creation, binding project setup, and known
  gotchas (simd math types, Engine::destroy pointer-to-pointer, CAMetalLayer setup).
  USE FOR: "filament ios binding", "filament objective-c wrapper", "FLTEngine",
  "filament xcframework", "filament ios maui", "filament c++ binding", "filament NativeReference".
  DO NOT USE FOR: Android binding (use filament-android-binding), cross-platform API
  design (use filament-maui-api-surface), surface integration (use filament-surface-integration).
---

# Filament iOS Binding for .NET MAUI

## Critical Blocker: C++ Only API

Filament on iOS provides **C++ headers only** — there are no Objective-C or Swift
wrappers anywhere in the repository. .NET MAUI iOS binding projects require an
Objective-C or Swift interface. This means **an Objective-C++ wrapper framework
must be authored before the binding project can be created**.

---

## Distribution

Filament iOS libraries are distributed via CocoaPods:
```
pod 'Filament', '~> 1.69.5'
```

Or via GitHub release tarballs:
```
https://github.com/google/filament/releases/download/v1.69.5/filament-v1.69.5-ios.tgz
```

- **Minimum iOS deployment target:** 11.0 (CocoaPods spec), 12.1+ recommended
- **Primary backend:** Metal (iOS 11+); OpenGL ES legacy/deprecated
- **Static libraries location:** `lib/universal/*.a` inside the extracted tgz
- **Headers location:** `include/filament/`, `include/backend/`, `include/utils/`, `include/math/`

---

## Recommended Wrapper Strategy: Objective-C++ Framework

Create a new `.xcframework` project with `.mm` (Objective-C++) source files that
expose the C++ Filament API through an Objective-C interface.

The iOS samples in the repository already demonstrate this pattern:
`ios/samples/HelloCocoaPods/HelloCocoaPods/ViewController.mm`

### Required C++ Includes in Wrapper

```objc
#include <filament/Engine.h>
#include <filament/Renderer.h>
#include <filament/Scene.h>
#include <filament/View.h>
#include <filament/Camera.h>
#include <filament/SwapChain.h>
#include <filament/Material.h>
#include <filament/MaterialInstance.h>
#include <filament/Texture.h>
#include <filament/RenderTarget.h>
#include <filament/IndirectLight.h>
#include <filament/Skybox.h>
#include <filament/RenderableManager.h>
#include <filament/TransformManager.h>
#include <filament/LightManager.h>
#include <utils/EntityManager.h>
```

### Objective-C++ Wrapper Skeleton (`FLTEngine`)

```objc
// FLTEngine.h
#import <Foundation/Foundation.h>
@class FLTRenderer, FLTScene, FLTView, FLTCamera, FLTSwapChain;
@class FLTTransformManager, FLTRenderableManager, FLTLightManager, FLTEntityManager;

typedef NS_ENUM(NSInteger, FLTBackend) {
    FLTBackendDefault = 0,
    FLTBackendMetal   = 3,
    FLTBackendOpenGL  = 1,
};

@interface FLTEngine : NSObject
+ (instancetype)createWithBackend:(FLTBackend)backend;
- (void)destroy;
- (FLTRenderer *)createRenderer;
- (FLTScene *)createScene;
- (FLTView *)createView;
- (FLTCamera *)createCamera;
- (FLTSwapChain *)createSwapChainFromLayer:(void *)nativeLayer;  // CAMetalLayer*
- (void)destroyRenderer:(FLTRenderer *)renderer;
- (void)destroyScene:(FLTScene *)scene;
- (void)destroyView:(FLTView *)view;
- (void)destroyCamera:(FLTCamera *)camera;
- (void)destroySwapChain:(FLTSwapChain *)swapChain;
- (void)flushAndWait;
- (FLTTransformManager *)transformManager;
- (FLTRenderableManager *)renderableManager;
- (FLTLightManager *)lightManager;
@end
```

```objc
// FLTEngine.mm
#import "FLTEngine.h"
#include <filament/Engine.h>
using namespace filament;

@implementation FLTEngine {
    Engine *_engine;
}

+ (instancetype)createWithBackend:(FLTBackend)backend {
    FLTEngine *wrapper = [[FLTEngine alloc] init];
    wrapper->_engine = Engine::create((Engine::Backend)backend);
    return wrapper;
}

- (void)destroy {
    // Note: Engine::destroy takes pointer-to-pointer and sets to null
    Engine::destroy(&_engine);
}

- (FLTSwapChain *)createSwapChainFromLayer:(void *)nativeLayer {
    // nativeLayer must be (__bridge void*)caMetalLayer
    // CAMetalLayer must be configured with correct pixelFormat before calling this
    auto *sc = _engine->createSwapChain(nativeLayer);
    return [[FLTSwapChain alloc] initWithNative:sc engine:self];
}

- (void)flushAndWait {
    _engine->flushAndWait();
}
@end
```

---

## Wrapper Class Mapping

| C++ Class | Objective-C Wrapper | Notes |
|---|---|---|
| `filament::Engine` | `FLTEngine` | Lifecycle, factory for all other objects |
| `filament::Renderer` | `FLTRenderer` | `beginFrame`/`render`/`endFrame` |
| `filament::View` | `FLTView` | Viewport, scene, camera, post-process options |
| `filament::Scene` | `FLTScene` | `addEntities`, `setIndirectLight`, `setSkybox` |
| `filament::Camera` | `FLTCamera` | `setProjection`, `lookAt` |
| `filament::SwapChain` | `FLTSwapChain` | Wraps CAMetalLayer pointer |
| `filament::Material` | `FLTMaterial` | `createInstance`, parameter metadata |
| `filament::MaterialInstance` | `FLTMaterialInstance` | `setParameter` for all types |
| `filament::Texture` | `FLTTexture` | Builder + `setImage` |
| `filament::RenderTarget` | `FLTRenderTarget` | Off-screen rendering |
| `utils::EntityManager` | `FLTEntityManager` | `create`/`destroy` entity IDs (uint32) |
| `filament::TransformManager` | `FLTTransformManager` | `setTransform`, `create` |
| `filament::RenderableManager` | `FLTRenderableManager` | Builder pattern, geometry + material |
| `filament::LightManager` | `FLTLightManager` | Builder pattern |
| `filament::IndirectLight` | `FLTIndirectLight` | Builder pattern |
| `filament::Skybox` | `FLTSkybox` | Builder pattern |
| `filament::VertexBuffer` | `FLTVertexBuffer` | Builder + `setBufferAt` |
| `filament::IndexBuffer` | `FLTIndexBuffer` | Builder + `setBuffer` |

---

## Building the XCFramework

```bash
# Archive for device and simulator
xcodebuild archive \
  -scheme FilamentWrapper \
  -destination "generic/platform=iOS" \
  -archivePath ./build/ios.xcarchive

xcodebuild archive \
  -scheme FilamentWrapper \
  -destination "generic/platform=iOS Simulator" \
  -archivePath ./build/ios-sim.xcarchive

# Merge into XCFramework
xcodebuild -create-xcframework \
  -framework ./build/ios.xcarchive/Products/Library/Frameworks/FilamentWrapper.framework \
  -framework ./build/ios-sim.xcarchive/Products/Library/Frameworks/FilamentWrapper.framework \
  -output ./FilamentWrapper.xcframework
```

---

## Generating Bindings with Objective Sharpie

```bash
sharpie bind \
  -sdk iphoneos \
  -o ApiDefinitions \
  -n FilamentBinding \
  FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTEngine.h \
  FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTRenderer.h \
  FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTView.h \
  # ... all wrapper headers
```

Review and clean up the generated `ApiDefinitions.cs` before using it.

---

## iOS Binding Project Setup

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-ios</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <NativeReference Include="Native\FilamentWrapper.xcframework">
      <Kind>Framework</Kind>
      <SmartLink>false</SmartLink>
    </NativeReference>
  </ItemGroup>
</Project>
```

---

## Known Gotchas

1. **`Engine::destroy(&engine)` takes a pointer-to-pointer** and sets it to `null`.
   The Objective-C++ wrapper must store `Engine*` as a member and pass `&_engine` —
   do NOT pass a local variable.

2. **Math types (`math::float3`, `math::mat4f`) are C++ templates** — they cannot be
   exposed through Objective-C headers. Use `simd_float3`, `simd_float4x4` from
   `<simd/simd.h>` (part of Metal/Accelerate) as substitutes in the wrapper interface.

3. **`CAMetalLayer` must be configured** with the correct `pixelFormat`
   (typically `MTLPixelFormatBGRA8Unorm`) before being passed to `createSwapChain()`.
   Failure to do this causes rendering artifacts or crashes.

4. **ARM64 simulator is excluded** in the CocoaPods spec:
   `EXCLUDED_ARCHS[sdk=iphonesimulator*] = arm64`
   Development on Apple Silicon Macs requires either x86_64 simulation or device builds.

5. **Materials are byte arrays** — load `.mat` files from the app bundle using
   `NSBundle.MainBundle.PathForResource` and pass the bytes to the wrapper.
   Filament has no file-path material loading API.

6. **Entity is `uint32_t`**, not an object — expose as `uint` in the Objective-C wrapper
   and as `uint` in C# (not a class with a native pointer handle).

7. **`libc++` must be linked** — add `spec.libraries = 'c++'` if distributing as a
   CocoaPod, or add `-lc++` to the Xcode linker flags.
