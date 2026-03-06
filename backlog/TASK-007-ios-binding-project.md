# TASK-007: iOS Binding Project from XCFramework

**Phase:** 3 — iOS Binding
**Estimated Effort:** 3–5 days
**Depends On:** TASK-006
**Relevant Skills:** `filament-ios-binding`, `filament-maui-project-structure`

## Objective

Create the `FilamentBinding.iOS` .NET MAUI iOS binding library project that wraps the `FilamentWrapper.xcframework` produced in TASK-006. This involves running Objective Sharpie to auto-generate an initial `ApiDefinitions.cs`, hand-editing the output to correct naming and attribute issues, and verifying the binding project compiles cleanly. The resulting `FilamentBinding.iOS.dll` is the iOS counterpart to `FilamentBinding.Android.dll`.

## Prerequisites

- TASK-006 complete — `FilamentWrapper.xcframework` exists in `maui/FilamentBinding.iOS/Native/`
- macOS development machine
- Objective Sharpie installed: `https://aka.ms/sharpie`
- .NET 10 SDK with MAUI workload and iOS SDK installed
- Xcode command line tools installed

## Deliverables

- `maui/FilamentBinding.iOS/FilamentBinding.iOS.csproj` — .NET MAUI iOS binding library project
- `maui/FilamentBinding.iOS/ApiDefinitions.cs` — C# binding declarations (Sharpie-generated, hand-edited)
- `maui/FilamentBinding.iOS/StructsAndEnums.cs` — C# enums mirroring `FLTBackend` and other enumerations
- `maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework` — the XCFramework from TASK-006
- `dotnet build` producing `FilamentBinding.iOS.dll` without errors

## Detailed Steps

### Step 1: Create the project and folder structure

```bash
mkdir -p maui/FilamentBinding.iOS/Native
```

Copy the XCFramework output from TASK-006:
```bash
cp -r maui/FilamentWrapper.iOS/FilamentWrapper.xcframework \
      maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework
```

Create `maui/FilamentBinding.iOS/FilamentBinding.iOS.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-ios</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>FilamentBinding.iOS</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <NativeReference Include="Native\FilamentWrapper.xcframework">
      <Kind>Framework</Kind>
      <SmartLink>false</SmartLink>
    </NativeReference>
  </ItemGroup>

  <ItemGroup>
    <ObjcBindingApiDefinition Include="ApiDefinitions.cs" />
    <ObjcBindingCoreSource Include="StructsAndEnums.cs" />
  </ItemGroup>
</Project>
```

### Step 2: Run Objective Sharpie to generate initial bindings

```bash
sharpie bind \
  -sdk iphoneos \
  -o SharpieOutput \
  -n FilamentBinding \
  maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTEngine.h \
  maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTRenderer.h \
  maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTView.h \
  maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTScene.h \
  maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTCamera.h \
  maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTSwapChain.h \
  maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTMaterial.h \
  maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTMaterialInstance.h \
  maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTTexture.h \
  maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTRenderTarget.h \
  maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTEntityManager.h \
  maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTTransformManager.h \
  maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTRenderableManager.h \
  maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTLightManager.h \
  maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTIndirectLight.h \
  maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTSkybox.h \
  maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTVertexBuffer.h \
  maui/FilamentBinding.iOS/Native/FilamentWrapper.xcframework/ios-arm64/FilamentWrapper.framework/Headers/FLTIndexBuffer.h
```

Review `SharpieOutput/ApiDefinitions.cs` and `SharpieOutput/StructsAndEnums.cs`. Copy them into the project:

```bash
cp SharpieOutput/ApiDefinitions.cs maui/FilamentBinding.iOS/ApiDefinitions.cs
cp SharpieOutput/StructsAndEnums.cs maui/FilamentBinding.iOS/StructsAndEnums.cs
```

### Step 3: Hand-edit ApiDefinitions.cs

Sharpie output requires manual review and cleanup. Common fixes:

1. **Remove `[Verify(...)]` attributes** that Sharpie emits when it is uncertain. Review each one and either keep or adjust the binding, then delete the attribute.

2. **Fix `FLTEngine.Create` static factory method** — Sharpie may map `+createWithBackend:` incorrectly. Ensure it becomes:
   ```csharp
   [Static]
   [Export("createWithBackend:")]
   FLTEngine Create(FLTBackend backend);
   ```

3. **Ensure `simd_float4x4` parameters are mapped to `Matrix4x4`** — Sharpie should auto-map `simd_float4x4` to `OpenTK.Matrix4` or `System.Numerics.Matrix4x4`. Verify and adjust if needed.

4. **Fix `NSData` parameters** — Sharpie maps `NSData *` to `NSData` in C#. This is correct.

5. **Ensure `uint32_t` entity IDs map to `uint`** (not wrapped in an object):
   ```csharp
   [Export("create")]
   uint Create();

   [Export("destroy:")]
   void Destroy(uint entity);
   ```

6. **Rename `FLTEngine.Destroy` to avoid collision** with the `IDisposable.Dispose` pattern if needed.

### Step 4: Author StructsAndEnums.cs

```csharp
// StructsAndEnums.cs
namespace FilamentBinding.iOS;

public enum FLTBackend : long
{
    Default = 0,
    OpenGL  = 1,
    Vulkan  = 2,
    Metal   = 3,
}
```

Add other enums from the wrapper headers (texture formats, light types, etc.) as they appear.

### Step 5: Build and fix remaining errors

```bash
cd maui/FilamentBinding.iOS
dotnet build -c Debug
```

Iterate on `ApiDefinitions.cs` to resolve:
- `CS0234` — missing type references (check namespace usage)
- `MT5211` — native linking failures (check `NativeReference` SmartLink setting)
- `BI1000` — binding definition issues (Sharpie `[Verify]` not cleaned up)

### Step 6: Verify the generated API surface

In a scratch test project referencing `FilamentBinding.iOS.dll`, confirm these types are accessible:

```csharp
using FilamentBinding.iOS;

var engine = FLTEngine.Create(FLTBackend.Metal);
var renderer = engine.CreateRenderer();
var scene = engine.CreateScene();
var view = engine.CreateView();
var camera = engine.CreateCamera();
engine.FlushAndWait();
engine.Destroy();
```

## Acceptance Criteria

- [ ] `FilamentBinding.iOS.csproj` exists targeting `net10.0-ios`
- [ ] `NativeReference` pointing to `FilamentWrapper.xcframework` is correctly configured
- [ ] `ApiDefinitions.cs` exists with all 18 wrapper classes bound (no unresolved `[Verify]` attributes)
- [ ] `StructsAndEnums.cs` defines `FLTBackend` and any other required enumerations
- [ ] `dotnet build` produces `FilamentBinding.iOS.dll` with no errors
- [ ] `FLTEngine`, `FLTRenderer`, `FLTView`, `FLTScene`, `FLTCamera`, `FLTSwapChain`, `FLTMaterial`, `FLTMaterialInstance`, `FLTTexture`, `FLTEntityManager`, `FLTTransformManager`, `FLTRenderableManager` are all accessible as C# classes
- [ ] `FLTEntityManager.Create()` returns `uint` (not an object)
- [ ] `FLTTransformManager.SetTransform:forEntity:` accepts a matrix type, not raw floats

## Reference

- See `.github/skills/filament-ios-binding/SKILL.md` — binding project setup, Sharpie command, known gotchas
- See `.github/skills/filament-maui-project-structure/SKILL.md` — `FilamentBinding.iOS` project layout
- Objective Sharpie: `https://learn.microsoft.com/en-us/dotnet/ios/binding-objective-c/walkthrough`
- iOS binding docs: `https://learn.microsoft.com/en-us/dotnet/ios/binding-objective-c/`
- TASK-006 output: `maui/FilamentWrapper.iOS/FilamentWrapper.xcframework`
