# Filament .NET MAUI Binding — Planning Agent Notes

These notes supplement the main assessment report (`docs/maui-binding-assessment.md`) and are intended for an implementation planning agent.

---

## Quick Facts

| Item | Value |
|---|---|
| Filament version | 1.69.5 |
| Android AAR group | `com.google.android.filament` |
| Android Maven Central | `com.google.android.filament:filament-android:1.69.5` |
| iOS CocoaPods | `pod 'Filament', '~> 1.69.5'` |
| iOS static libs location (prebuilt) | `lib/universal/` in extracted tgz release |
| iOS headers location | `include/filament/`, `include/backend/`, `include/utils/`, etc. |
| iOS min deployment target | 11.0 (CocoaPods); 12.1 recommended |
| Android min SDK | API 21 (Lollipop) recommended |
| C++ standard | C++20 (used internally) |
| iOS rendering backend | Metal (primary), OpenGL ES (legacy) |
| Android rendering backend | OpenGL ES 3.0, Vulkan 1.0 |

---

## Critical Implementation Notes

### Android Binding

1. **AAR download URL pattern:**
   ```
   https://repo1.maven.org/maven2/com/google/android/filament/filament-android/1.69.5/filament-android-1.69.5.aar
   ```
   Similarly for `gltfio-android`, `filamat-android`, `filament-utils-android`.

2. **Initialization:** The binding must call `Filament.init()` before any other call. In C# this maps to `FilamentBinding.Filament.Init()`. This loads `libfilament-jni.so`.

3. **JNI Object Handle Pattern:** Every Java class stores a `long mNativeObject`. The C# binding classes will hold `IntPtr` or `long` handle similarly. Avoid double-free via careful `Dispose()` patterns.

4. **Metadata transforms needed:**
   - Rename `android` subpackage classes to avoid conflicts (e.g., `AndroidPlatform` → `FilamentAndroidPlatform`).
   - Mark deprecated classes (`ToneMapper`) as `[Obsolete]`.
   - Expose `UiHelper.RendererCallback` as a C# interface or abstract class.

5. **Builder pattern conversion:** Many classes use Java inner `Builder` classes. In C# binding these become `Engine.Builder`, `Texture.Builder`, etc. These should map naturally but may need `Transforms/Metadata.xml` entries if the nested class names collide.

6. **gltfio dependency chain:**
   ```
   filament-android ← gltfio-android ← filament-utils-android
   ```
   When creating binding projects, follow this dependency order.

### iOS Binding

1. **No Objective-C wrappers exist.** The entire wrapper must be written from scratch. This is the biggest effort item.

2. **Sample pattern to follow:** `/ios/samples/HelloCocoaPods/HelloCocoaPods/ViewController.mm` — 238 lines showing complete minimal rendering. Use as reference for how to author the Objective-C++ wrapper.

3. **C++ includes needed in wrapper:**
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

4. **XCFramework build command (approximate):**
   ```bash
   xcodebuild archive -scheme FilamentWrapper -destination "generic/platform=iOS" -archivePath ./build/ios.xcarchive
   xcodebuild archive -scheme FilamentWrapper -destination "generic/platform=iOS Simulator" -archivePath ./build/ios-sim.xcarchive
   xcodebuild -create-xcframework \
     -framework ./build/ios.xcarchive/Products/Library/Frameworks/FilamentWrapper.framework \
     -framework ./build/ios-sim.xcarchive/Products/Library/Frameworks/FilamentWrapper.framework \
     -output ./FilamentWrapper.xcframework
   ```

5. **Objective-C++ wrapper skeleton for `FLTEngine`:**
   ```objc
   // FLTEngine.h
   #import <Foundation/Foundation.h>
   @class FLTRenderer, FLTScene, FLTView, FLTCamera, FLTSwapChain;

   @interface FLTEngine : NSObject
   + (instancetype)createWithBackend:(int)backend;
   - (void)destroy;
   - (FLTRenderer*)createRenderer;
   - (FLTScene*)createScene;
   - (FLTView*)createView;
   - (FLTCamera*)createCamera;
   - (FLTSwapChain*)createSwapChainFromLayer:(void*)nativeLayer;
   - (void)destroyRenderer:(FLTRenderer*)renderer;
   - (void)flushAndWait;
   @end

   // FLTEngine.mm
   #import "FLTEngine.h"
   #include <filament/Engine.h>
   using namespace filament;

   @implementation FLTEngine {
       Engine* _engine;
   }
   + (instancetype)createWithBackend:(int)backend {
       FLTEngine* wrapper = [[FLTEngine alloc] init];
       wrapper->_engine = Engine::create((Engine::Backend)backend);
       return wrapper;
   }
   - (void)destroy {
       Engine::destroy(&_engine);
       _engine = nullptr;
   }
   - (FLTSwapChain*)createSwapChainFromLayer:(void*)nativeLayer {
       // nativeLayer is (__bridge void*)caMetalLayer
       auto sc = _engine->createSwapChain(nativeLayer);
       return [[FLTSwapChain alloc] initWithNative:sc engine:self];
   }
   // ... etc
   @end
   ```

6. **Sharpie command to generate initial binding from wrapper headers:**
   ```bash
   sharpie bind -sdk iphoneos -o ApiDefinitions -n FilamentBinding \
     FilamentWrapper.framework/Headers/FLTEngine.h \
     FilamentWrapper.framework/Headers/FLTRenderer.h \
     ...
   ```

### Cross-Platform Library

1. **Project TFMs to use:**
   ```
   net10.0-android
   net10.0-ios
   ```

2. **`FilamentView` should be a `Microsoft.Maui.Controls.View` subclass** with platform-specific `IViewHandler` implementations:
   - Android handler: Creates `SurfaceView`, wraps `UiHelper`, manages render thread.
   - iOS handler: Creates `UIView` with `CAMetalLayer`, manages `CADisplayLink`.

3. **Render thread management (critical):** The `IFilamentEngine` must be created on a dedicated background thread (not the MAUI UI thread). All Filament calls (createSwapChain, beginFrame, render, endFrame) must be called from this same thread. The cross-platform library should encapsulate a `FilamentRenderThread` class.

4. **Memory lifecycle:** All `IDisposable` wrappers must call the native `destroy()` method in `Dispose()`. Use `SafeHandle` or `CriticalHandle` patterns if needed.

5. **Material assets:** Materials must be compiled per platform. Ship platform-specific material `.mat` files in the MAUI app's `Resources/Raw` folder and load them conditionally:
   ```csharp
   #if ANDROID
   var materialBytes = LoadAsset("materials/default.mat.android");
   #elif IOS
   var materialBytes = LoadAsset("materials/default.mat.ios");
   #endif
   ```

---

## Suggested Work Breakdown

| Phase | Task | Estimated Effort |
|---|---|---|
| 1 | Android AAR binding project (filament-android) | 3–5 days |
| 2 | Android binding cleanup (Metadata.xml, Additions) | 2–3 days |
| 3 | Core cross-platform interfaces (IFilamentEngine, etc.) | 2–3 days |
| 4 | Android implementation in Filament.Maui | 3–5 days |
| 5 | FilamentView MAUI control (Android) | 3–5 days |
| 6 | iOS Objective-C++ wrapper (FLTEngine, FLTRenderer, etc.) | 10–15 days |
| 7 | iOS binding project (ApiDefinitions.cs, StructsAndEnums.cs) | 3–5 days |
| 8 | iOS implementation in Filament.Maui | 3–5 days |
| 9 | FilamentView MAUI control (iOS) | 3–5 days |
| 10 | Sample app demonstrating cross-platform usage | 3–5 days |
| 11 | NuGet packaging | 1–2 days |
| **Total** | | **~35–55 days** |

---

## File Locations for Reference

| Item | Path |
|---|---|
| Android Java files | `android/filament-android/src/main/java/com/google/android/filament/` |
| Android JNI C++ files | `android/filament-android/src/main/cpp/` |
| Android gltfio Java files | `android/gltfio-android/src/main/java/com/google/android/filament/gltfio/` |
| Android utils Java files | `android/filament-utils-android/src/main/java/com/google/android/filament/utils/` |
| Android filamat Java files | `android/filamat-android/src/main/java/com/google/android/filament/filamat/` |
| Android build.gradle | `android/filament-android/build.gradle` |
| iOS CocoaPods spec | `ios/CocoaPods/Filament.podspec` |
| iOS sample (minimal) | `ios/samples/HelloCocoaPods/HelloCocoaPods/ViewController.mm` |
| iOS sample (gltf) | `ios/samples/hello-gltf/` |
| Core C++ headers | `filament/include/filament/` |
| Utils C++ headers | `libs/utils/include/utils/` |
| Math headers | `libs/math/include/math/` |
| Assessment report | `docs/maui-binding-assessment.md` |

---

## Known Gotchas

1. **`Entity` is an int (32-bit), not a class.** In Java it is `int entity = EntityManager.get().create()`. In C# this should be an `int` or a struct wrapper, not a class with a native pointer.

2. **Builder pattern in Java uses method chaining returning `this`.** The `Engine.Builder` must be properly mapped. The C# binding generator may need help with the fluent interface.

3. **`UiHelper.RendererCallback` is an interface in Java.** It must be exposed as an abstract class in C# bindings (Java interfaces with callbacks require special handling in Xamarin/MAUI bindings).

4. **Kotlin dependency in `filament-utils-android`.** The binding will include Kotlin stdlib in the AAR. This is normal but adds binary size and may require `kotlin-stdlib` to be excluded from the final MAUI app if it's already included transitively.

5. **iOS: The `math::` types (`float3`, `mat4f`, `quat`) are C++ template types.** They cannot be directly exposed via Objective-C. The wrapper must use `NSArray<NSNumber*>` or custom structs for these (e.g., `simd_float3`, `simd_float4x4` from Metal).

6. **iOS: CAMetalLayer must be configured with the correct pixel format** before being passed to `createSwapChain()`. Failure to do this is a common source of rendering artifacts.

7. **iOS: `Engine::destroy(&engine)` sets the pointer to null.** The wrapper must handle this pointer-to-pointer pattern carefully.

8. **Both platforms: Materials must be loaded as byte arrays.** There is no file path API; Filament takes raw `uint8_t*` (iOS C++) or `ByteBuffer` (Android Java).
