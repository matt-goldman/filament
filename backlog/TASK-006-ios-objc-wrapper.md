# TASK-006: iOS Objective-C++ Wrapper Framework

**Phase:** 3 — iOS Binding
**Estimated Effort:** 10–15 days
**Depends On:** None (can start independently; only TASK-007 depends on this)
**Relevant Skills:** `filament-ios-binding`, `filament-maui-project-structure`

## Objective

Author the `FilamentWrapper.iOS` Xcode framework project — a set of Objective-C++ (`.mm`) source files that wrap the C++ Filament API in an Objective-C interface. This is the highest-effort item in the entire binding project. Filament has **no Objective-C or Swift API on iOS**, so this wrapper must be written entirely from scratch. The resulting `.xcframework` becomes the input to the .NET MAUI iOS binding project in TASK-007.

## Prerequisites

- macOS development machine with Xcode 15+
- Filament 1.69.5 iOS static libraries, obtained via one of:
  - CocoaPods: `pod 'Filament', '~> 1.69.5'`
  - GitHub release tgz: `https://github.com/google/filament/releases/download/v1.69.5/filament-v1.69.5-ios.tgz`
- Reference the existing iOS sample at `ios/samples/HelloCocoaPods/HelloCocoaPods/ViewController.mm` — 238-line minimal rendering example demonstrating the C++ call pattern
- iOS deployment target: 12.1+ (12.1 recommended; 11.0 minimum per CocoaPods spec)
- `libc++` must be linked (`-lc++` in linker flags)

## Deliverables

- `maui/FilamentWrapper.iOS/FilamentWrapper.xcodeproj` — Xcode framework project
- **18 Objective-C++ wrapper classes** (18 `.h` and 18 `.mm` file pairs):
  1. `FLTEngine.h` / `FLTEngine.mm`
  2. `FLTRenderer.h` / `FLTRenderer.mm`
  3. `FLTView.h` / `FLTView.mm`
  4. `FLTScene.h` / `FLTScene.mm`
  5. `FLTCamera.h` / `FLTCamera.mm`
  6. `FLTSwapChain.h` / `FLTSwapChain.mm`
  7. `FLTMaterial.h` / `FLTMaterial.mm`
  8. `FLTMaterialInstance.h` / `FLTMaterialInstance.mm`
  9. `FLTTexture.h` / `FLTTexture.mm`
  10. `FLTRenderTarget.h` / `FLTRenderTarget.mm`
  11. `FLTEntityManager.h` / `FLTEntityManager.mm`
  12. `FLTTransformManager.h` / `FLTTransformManager.mm`
  13. `FLTRenderableManager.h` / `FLTRenderableManager.mm`
  14. `FLTLightManager.h` / `FLTLightManager.mm`
  15. `FLTIndirectLight.h` / `FLTIndirectLight.mm`
  16. `FLTSkybox.h` / `FLTSkybox.mm`
  17. `FLTVertexBuffer.h` / `FLTVertexBuffer.mm`
  18. `FLTIndexBuffer.h` / `FLTIndexBuffer.mm`
- `maui/FilamentWrapper.iOS/FilamentWrapper.h` — umbrella header importing all public headers
- `FilamentWrapper.xcframework` — built for `ios-arm64` and `ios-arm64_x86_64-simulator`

## Detailed Steps

### Step 1: Create the Xcode Framework Project

1. Open Xcode → File → New → Project → iOS → Framework
2. Product name: `FilamentWrapper`
3. Language: Objective-C
4. Set deployment target to iOS 12.1
5. In Build Settings:
   - `OTHER_LDFLAGS`: `-lc++ -lfilament -lbackend -lfilabridge -lfilaflat -lutils -libl -lgeometry -lsmol-v`
   - `LIBRARY_SEARCH_PATHS`: path to extracted Filament `.a` files (`lib/universal/` from tgz)
   - `HEADER_SEARCH_PATHS`: path to Filament include directories (`include/`)
   - `CLANG_CXX_LANGUAGE_STANDARD`: `c++17`
   - `EXCLUDED_ARCHS[sdk=iphonesimulator*]`: `arm64` (Apple Silicon simulator arm64 excluded per Filament's CocoaPods spec)

Link the following static libraries from `lib/universal/`:
```
libfilament.a      libbackend.a     libfilabridge.a
libfilaflat.a      libutils.a       libibl.a
libgeometry.a      libsmol-v.a
```

### Step 2: Author FLTEngine (most critical class)

**Required C++ includes in all `.mm` files:**
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

`FLTEngine.h`:
```objc
#import <Foundation/Foundation.h>
@class FLTRenderer, FLTScene, FLTView, FLTCamera, FLTSwapChain;
@class FLTTransformManager, FLTRenderableManager, FLTLightManager, FLTEntityManager;

typedef NS_ENUM(NSInteger, FLTBackend) {
    FLTBackendDefault  = 0,
    FLTBackendOpenGL   = 1,
    FLTBackendVulkan   = 2,
    FLTBackendMetal    = 3,
};

@interface FLTEngine : NSObject
/// Creates the Filament engine. Use FLTBackendMetal on iOS.
+ (instancetype)createWithBackend:(FLTBackend)backend;
- (void)destroy;
- (FLTRenderer *)createRenderer;
- (FLTScene *)createScene;
- (FLTView *)createView;
- (FLTCamera *)createCamera;
/// nativeLayer must be (__bridge void*)caMetalLayer.
/// CAMetalLayer must be configured (pixelFormat = MTLPixelFormatBGRA8Unorm) before calling.
- (FLTSwapChain *)createSwapChainFromLayer:(void *)nativeLayer;
- (void)destroyRenderer:(FLTRenderer *)renderer;
- (void)destroyScene:(FLTScene *)scene;
- (void)destroyView:(FLTView *)view;
- (void)destroyCamera:(FLTCamera *)camera;
- (void)destroySwapChain:(FLTSwapChain *)swapChain;
- (void)flushAndWait;
- (FLTTransformManager *)transformManager;
- (FLTRenderableManager *)renderableManager;
- (FLTLightManager *)lightManager;
- (FLTEntityManager *)entityManager;
@end
```

`FLTEngine.mm`:
```objc
#import "FLTEngine.h"
#import "FLTRenderer.h"
#import "FLTScene.h"
#import "FLTView.h"
#import "FLTCamera.h"
#import "FLTSwapChain.h"
#import "FLTTransformManager.h"
#import "FLTRenderableManager.h"
#import "FLTLightManager.h"
#import "FLTEntityManager.h"
#include <filament/Engine.h>
using namespace filament;

@implementation FLTEngine {
    Engine *_engine;  // Raw C++ pointer; Engine::destroy(&_engine) zeroes it
}

+ (instancetype)createWithBackend:(FLTBackend)backend {
    FLTEngine *w = [[FLTEngine alloc] init];
    w->_engine = Engine::create((Engine::Backend)backend);
    return w;
}

- (void)destroy {
    // CRITICAL: Engine::destroy takes pointer-to-pointer and sets the pointer to null
    Engine::destroy(&_engine);
}

- (Engine *)nativeEngine { return _engine; }

- (FLTRenderer *)createRenderer {
    return [[FLTRenderer alloc] initWithNative:_engine->createRenderer() engine:self];
}
- (FLTScene *)createScene {
    return [[FLTScene alloc] initWithNative:_engine->createScene() engine:self];
}
- (FLTView *)createView {
    return [[FLTView alloc] initWithNative:_engine->createView() engine:self];
}
- (FLTCamera *)createCamera {
    utils::Entity camEntity = utils::EntityManager::get().create();
    return [[FLTCamera alloc] initWithNative:_engine->createCamera(camEntity) engine:self];
}
- (FLTSwapChain *)createSwapChainFromLayer:(void *)nativeLayer {
    // nativeLayer must be a CAMetalLayer configured with MTLPixelFormatBGRA8Unorm
    SwapChain *sc = _engine->createSwapChain(nativeLayer);
    return [[FLTSwapChain alloc] initWithNative:sc engine:self];
}
- (void)destroyRenderer:(FLTRenderer *)r { _engine->destroy([r nativeRenderer]); }
- (void)destroyScene:(FLTScene *)s     { _engine->destroy([s nativeScene]); }
- (void)destroyView:(FLTView *)v       { _engine->destroy([v nativeView]); }
- (void)destroyCamera:(FLTCamera *)c   { _engine->destroy([c nativeCamera]); }
- (void)destroySwapChain:(FLTSwapChain *)sc { _engine->destroy([sc nativeSwapChain]); }
- (void)flushAndWait { _engine->flushAndWait(); }

- (FLTTransformManager *)transformManager {
    return [[FLTTransformManager alloc] initWithNative:&_engine->getTransformManager() engine:self];
}
- (FLTRenderableManager *)renderableManager {
    return [[FLTRenderableManager alloc] initWithNative:&_engine->getRenderableManager() engine:self];
}
- (FLTLightManager *)lightManager {
    return [[FLTLightManager alloc] initWithNative:&_engine->getLightManager() engine:self];
}
- (FLTEntityManager *)entityManager {
    return [[FLTEntityManager alloc] init];
}
@end
```

### Step 3: Author FLTRenderer

`FLTRenderer.h`:
```objc
#import <Foundation/Foundation.h>
@class FLTSwapChain, FLTView, FLTEngine;

@interface FLTRenderer : NSObject
- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine;
- (void *)nativeRenderer;
- (BOOL)beginFrame:(FLTSwapChain *)swapChain;
- (void)render:(FLTView *)view;
- (void)endFrame;
@end
```

`FLTRenderer.mm`:
```objc
#import "FLTRenderer.h"
#import "FLTSwapChain.h"
#import "FLTView.h"
#include <filament/Renderer.h>
#include <filament/SwapChain.h>
using namespace filament;

@implementation FLTRenderer {
    Renderer *_renderer;
}
- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine {
    self = [super init];
    _renderer = (Renderer *)native;
    return self;
}
- (void *)nativeRenderer { return _renderer; }
- (BOOL)beginFrame:(FLTSwapChain *)swapChain {
    return _renderer->beginFrame((SwapChain *)[swapChain nativeSwapChain]);
}
- (void)render:(FLTView *)view {
    _renderer->render((View *)[view nativeView]);
}
- (void)endFrame { _renderer->endFrame(); }
@end
```

### Step 4: Author FLTView, FLTScene, FLTCamera, FLTSwapChain

Follow the same pattern as `FLTRenderer`. Key signatures:

**FLTView.h** (key methods):
```objc
@interface FLTView : NSObject
- (void)setScene:(FLTScene *)scene;
- (void)setCamera:(FLTCamera *)camera;
- (void)setViewportLeft:(int)left bottom:(int)bottom width:(uint32_t)width height:(uint32_t)height;
- (void)setClearColorRed:(float)r green:(float)g blue:(float)b alpha:(float)a;
- (void)setPostProcessingEnabled:(BOOL)enabled;
- (void *)nativeView;
@end
```

**FLTCamera.h** (key methods — use `double`, not C++ `math::` types):
```objc
@interface FLTCamera : NSObject
- (void)setProjectionFov:(double)fovDegrees aspect:(double)aspect near:(double)near far:(double)far;
- (void)lookAtEyeX:(double)ex eyeY:(double)ey eyeZ:(double)ez
           centerX:(double)cx centerY:(double)cy centerZ:(double)cz
                upX:(double)ux upY:(double)uy upZ:(double)uz;
- (void *)nativeCamera;
@end
```

In the `.mm` implementation, convert scalar parameters to `math::float3` / `math::double3`:
```objc
#include <math/vec3.h>
_camera->lookAt(
    {(float)ex, (float)ey, (float)ez},
    {(float)cx, (float)cy, (float)cz},
    {(float)ux, (float)uy, (float)uz}
);
```

**FLTSwapChain.h**:
```objc
@interface FLTSwapChain : NSObject
- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine;
- (void *)nativeSwapChain;
@end
```

### Step 5: Author FLTEntityManager (uint32_t entity IDs)

```objc
// FLTEntityManager.h
@interface FLTEntityManager : NSObject
/// Returns a new entity ID (uint32). Entity is NOT an object — it is a raw integer.
- (uint32_t)create;
- (void)destroy:(uint32_t)entity;
@end

// FLTEntityManager.mm
#include <utils/EntityManager.h>
@implementation FLTEntityManager
- (uint32_t)create {
    utils::Entity e = utils::EntityManager::get().create();
    return e.getId();
}
- (void)destroy:(uint32_t)entity {
    utils::Entity e = utils::Entity::import(entity);
    utils::EntityManager::get().destroy(e);
}
@end
```

### Step 6: Author FLTTransformManager (simd math types)

C++ `math::mat4f` cannot pass through Objective-C headers. Use `simd_float4x4` from `<simd/simd.h>`:

```objc
// FLTTransformManager.h
#import <simd/simd.h>
@interface FLTTransformManager : NSObject
- (void)createComponent:(uint32_t)entity;
- (void)setTransform:(simd_float4x4)transform forEntity:(uint32_t)entity;
@end

// FLTTransformManager.mm
#include <filament/TransformManager.h>
#include <math/mat4.h>
using namespace filament;

@implementation FLTTransformManager {
    TransformManager *_mgr;
}
- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine {
    _mgr = (TransformManager *)native;
    return self;
}
- (void)createComponent:(uint32_t)entity {
    _mgr->create(utils::Entity::import(entity));
}
- (void)setTransform:(simd_float4x4)m forEntity:(uint32_t)entity {
    // Convert simd_float4x4 to filament math::mat4f (column-major)
    math::mat4f mat(
        math::float4{m.columns[0].x, m.columns[0].y, m.columns[0].z, m.columns[0].w},
        math::float4{m.columns[1].x, m.columns[1].y, m.columns[1].z, m.columns[1].w},
        math::float4{m.columns[2].x, m.columns[2].y, m.columns[2].z, m.columns[2].w},
        math::float4{m.columns[3].x, m.columns[3].y, m.columns[3].z, m.columns[3].w}
    );
    auto i = _mgr->getInstance(utils::Entity::import(entity));
    _mgr->setTransform(i, mat);
}
@end
```

### Step 7: Author FLTMaterial and FLTMaterialInstance

Materials are loaded from byte arrays — Filament has no file-path API:

```objc
// FLTMaterial.h
@interface FLTMaterial : NSObject
+ (instancetype)buildWithEngine:(FLTEngine *)engine data:(NSData *)matData;
- (FLTMaterialInstance *)createInstance;
- (void *)nativeMaterial;
@end

// FLTMaterial.mm
#include <filament/Material.h>
@implementation FLTMaterial {
    filament::Material *_material;
}
+ (instancetype)buildWithEngine:(FLTEngine *)engine data:(NSData *)matData {
    FLTMaterial *w = [[FLTMaterial alloc] init];
    w->_material = filament::Material::Builder()
        .package(matData.bytes, matData.length)
        .build(*[engine nativeEngine]);
    return w;
}
- (FLTMaterialInstance *)createInstance {
    return [[FLTMaterialInstance alloc] initWithNative:_material->createInstance()];
}
@end
```

`FLTMaterialInstance.h` (key method — use `simd_float4` for float4 parameters):
```objc
#import <simd/simd.h>
@interface FLTMaterialInstance : NSObject
- (void)setFloatParameter:(NSString *)name value:(float)value;
- (void)setFloat4Parameter:(NSString *)name value:(simd_float4)value;
- (void)setTextureParameter:(NSString *)name texture:(FLTTexture *)texture;
@end
```

### Step 8: Author remaining classes

Follow the same pattern for the remaining classes:

**FLTRenderableManager** — Builder pattern wrapping `filament::RenderableManager::Builder`. The Builder takes geometry buffers, bone count, and material instances. This is the most complex Builder to wrap.

**FLTLightManager** — `FLTLightManager.Builder` for directional/point/spot lights. Builder sets intensity, color, direction via `double` / `simd_float3` parameters.

**FLTIndirectLight** — `FLTIndirectLight.Builder` taking IBL reflection cube texture and irradiance data.

**FLTSkybox** — `FLTSkybox.Builder` taking a cube texture or color.

**FLTVertexBuffer** — `FLTVertexBuffer.Builder` specifying vertex count, buffer count, attributes. `setBufferAt:engine:index:data:` takes `NSData`.

**FLTIndexBuffer** — `FLTIndexBuffer.Builder` specifying index count and type. `setBuffer:engine:data:` takes `NSData`.

**FLTTexture** — `FLTTexture.Builder` for 2D/cubemap textures. `setImage:engine:level:data:` takes `NSData`.

**FLTRenderTarget** — `FLTRenderTarget.Builder` specifying attachment textures.

### Step 9: Author the umbrella header

`FilamentWrapper.h`:
```objc
#import <Foundation/Foundation.h>
#import "FLTEngine.h"
#import "FLTRenderer.h"
#import "FLTView.h"
#import "FLTScene.h"
#import "FLTCamera.h"
#import "FLTSwapChain.h"
#import "FLTMaterial.h"
#import "FLTMaterialInstance.h"
#import "FLTTexture.h"
#import "FLTRenderTarget.h"
#import "FLTEntityManager.h"
#import "FLTTransformManager.h"
#import "FLTRenderableManager.h"
#import "FLTLightManager.h"
#import "FLTIndirectLight.h"
#import "FLTSkybox.h"
#import "FLTVertexBuffer.h"
#import "FLTIndexBuffer.h"
```

### Step 10: Build the XCFramework

```bash
# Archive for device (arm64)
xcodebuild archive \
  -scheme FilamentWrapper \
  -destination "generic/platform=iOS" \
  -archivePath ./build/ios.xcarchive \
  SKIP_INSTALL=NO BUILD_LIBRARY_FOR_DISTRIBUTION=YES

# Archive for simulator (x86_64 + arm64 if supported; exclude arm64 for simulator per Filament spec)
xcodebuild archive \
  -scheme FilamentWrapper \
  -destination "generic/platform=iOS Simulator" \
  -archivePath ./build/ios-sim.xcarchive \
  SKIP_INSTALL=NO BUILD_LIBRARY_FOR_DISTRIBUTION=YES \
  EXCLUDED_ARCHS="arm64"

# Create XCFramework from both archives
xcodebuild -create-xcframework \
  -framework ./build/ios.xcarchive/Products/Library/Frameworks/FilamentWrapper.framework \
  -framework ./build/ios-sim.xcarchive/Products/Library/Frameworks/FilamentWrapper.framework \
  -output ./FilamentWrapper.xcframework
```

Copy `FilamentWrapper.xcframework` to:
```
maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework
```

### Step 11: Verify with a minimal Objective-C test

Write a minimal Objective-C test (can be a standalone iOS app or unit test target) that creates an engine, renderer, view, scene, and camera, and calls `beginFrame` / `endFrame` on a small off-screen surface. This validates the wrapper builds and links correctly before TASK-007 proceeds.

```objc
FLTEngine *engine = [FLTEngine createWithBackend:FLTBackendMetal];
FLTRenderer *renderer = [engine createRenderer];
FLTScene *scene = [engine createScene];
FLTView *view = [engine createView];
FLTCamera *camera = [engine createCamera];
[view setScene:scene];
[view setCamera:camera];
[engine flushAndWait];
[engine destroy];
```

## Acceptance Criteria

- [ ] All 18 Objective-C++ wrapper classes exist with `.h` and `.mm` files
- [ ] `FilamentWrapper.xcframework` builds successfully for both `ios-arm64` and `ios-arm64_x86_64-simulator` slices
- [ ] `FLTEngine.createWithBackend:` creates an engine without crashing
- [ ] `FLTEngine.createSwapChainFromLayer:` accepts a `CAMetalLayer` pointer correctly
- [ ] `Engine::destroy(&_engine)` (pointer-to-pointer) is used in `FLTEngine.destroy` — NOT `Engine::destroy(_engine)`
- [ ] All math types in Objective-C headers use `simd_float3` / `simd_float4x4` (NOT C++ `math::float3` or `math::mat4f`)
- [ ] `FLTEntityManager.create` returns `uint32_t`, not an object
- [ ] `FLTMaterial.buildWithEngine:data:` accepts `NSData *` (byte array) — no file-path API
- [ ] `FLTTransformManager.setTransform:forEntity:` accepts `simd_float4x4`
- [ ] Umbrella header `FilamentWrapper.h` imports all 18 public headers
- [ ] Minimal Objective-C test creating engine → renderer → view → scene → camera passes without crash

## Reference

- See `.github/skills/filament-ios-binding/SKILL.md` — full `FLTEngine` skeleton, wrapper class table, XCFramework build commands, known gotchas
- See `docs/maui-binding-notes.md` — iOS binding critical notes (pointer-to-pointer, simd types, CAMetalLayer setup)
- Filament iOS sample (minimal): `ios/samples/HelloCocoaPods/HelloCocoaPods/ViewController.mm`
- Filament iOS sample (glTF): `ios/samples/hello-gltf/`
- Core C++ headers: `filament/include/filament/`
- Utils C++ headers: `libs/utils/include/utils/`
- Math headers: `libs/math/include/math/`
- CocoaPods spec: `ios/CocoaPods/Filament.podspec`
- iOS release tgz: `https://github.com/google/filament/releases/download/v1.69.5/filament-v1.69.5-ios.tgz`
