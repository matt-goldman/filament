# TASK-001: Android AAR Binding Project Setup

**Phase:** 1 — Android Binding
**Estimated Effort:** 3–5 days
**Depends On:** None
**Relevant Skills:** `filament-android-binding`, `filament-maui-project-structure`

## Objective

Create the `FilamentBinding.Android` .NET MAUI `AndroidBindingLibrary` project that wraps the Filament AARs from Maven Central. This is the foundation for all Android-side Filament usage and must be completed before Android interface implementations or the Android `FilamentView` handler can be built.

## Prerequisites

- .NET 10 SDK with MAUI workload installed (`dotnet workload install maui`)
- Android SDK (API 21+) installed
- Internet access to download AARs from Maven Central (or AARs pre-downloaded)

## Deliverables

- `maui/FilamentBinding.Android/FilamentBinding.Android.csproj` — `AndroidBindingLibrary` project targeting `net10.0-android`
- `maui/FilamentBinding.Android/Jars/filament-android-1.69.5.aar` — core AAR
- `maui/FilamentBinding.Android/Jars/gltfio-android-1.69.5.aar` — glTF loader AAR (optional but recommended)
- `maui/FilamentBinding.Android/Jars/filamat-android-1.69.5.aar` — runtime material compiler AAR (optional)
- `maui/FilamentBinding.Android/Jars/filament-utils-android-1.69.5.aar` — camera utils AAR (optional)
- `maui/FilamentBinding.Android/Transforms/Metadata.xml` — initial (empty or minimal) transforms file
- `maui/FilamentBinding.Android/Additions/FilamentExtensions.cs` — placeholder for C# additions
- Successful `dotnet build` producing `FilamentBinding.Android.dll`

## Detailed Steps

### Step 1: Create the solution and project skeleton

```bash
mkdir -p maui/FilamentBinding.Android/Jars
mkdir -p maui/FilamentBinding.Android/Transforms
mkdir -p maui/FilamentBinding.Android/Additions
```

Create `maui/FilamentBinding.Android/FilamentBinding.Android.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-android</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>Com.Google.Android.Filament</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <!-- Core rendering engine (required) -->
    <LibraryProjectZip Include="Jars\filament-android-1.69.5.aar" />
    <!-- glTF 2.0 loader (optional) -->
    <LibraryProjectZip Include="Jars\gltfio-android-1.69.5.aar"
                       Condition="Exists('Jars\gltfio-android-1.69.5.aar')" />
    <!-- Runtime material compiler (optional) -->
    <LibraryProjectZip Include="Jars\filamat-android-1.69.5.aar"
                       Condition="Exists('Jars\filamat-android-1.69.5.aar')" />
    <!-- Camera utilities / IBL preprocessing (optional) -->
    <LibraryProjectZip Include="Jars\filament-utils-android-1.69.5.aar"
                       Condition="Exists('Jars\filament-utils-android-1.69.5.aar')" />
  </ItemGroup>
</Project>
```

### Step 2: Download the AARs from Maven Central

```bash
BASE=https://repo1.maven.org/maven2/com/google/android/filament
VERSION=1.69.5
JARS=maui/FilamentBinding.Android/Jars

curl -L "$BASE/filament-android/$VERSION/filament-android-$VERSION.aar" \
     -o "$JARS/filament-android-$VERSION.aar"

curl -L "$BASE/gltfio-android/$VERSION/gltfio-android-$VERSION.aar" \
     -o "$JARS/gltfio-android-$VERSION.aar"

curl -L "$BASE/filamat-android/$VERSION/filamat-android-$VERSION.aar" \
     -o "$JARS/filamat-android-$VERSION.aar"

curl -L "$BASE/filament-utils-android/$VERSION/filament-utils-android-$VERSION.aar" \
     -o "$JARS/filament-utils-android-$VERSION.aar"
```

Note the AAR dependency order (important for binding):
```
filament-android  ←  gltfio-android  ←  filament-utils-android
filament-android  ←  filamat-android
```

### Step 3: Add a minimal Transforms/Metadata.xml

Create `maui/FilamentBinding.Android/Transforms/Metadata.xml` with initial content (full cleanup happens in TASK-002):

```xml
<metadata>
  <!-- Placeholder — full transforms added in TASK-002 -->
</metadata>
```

### Step 4: Add placeholder Additions file

Create `maui/FilamentBinding.Android/Additions/FilamentExtensions.cs`:

```csharp
// Placeholder for C#-friendly extension methods and partial class additions.
// Populated in TASK-002.
```

### Step 5: Build and inspect the generated bindings

```bash
cd maui/FilamentBinding.Android
dotnet build -c Debug
```

After a successful build, inspect generated C# API with:

```bash
# List all generated types to check for name collisions or missing classes
dotnet tool run dotnet-ilverify ./bin/Debug/net10.0-android/FilamentBinding.Android.dll
```

Or open the project in Visual Studio / Rider to browse IntelliSense. Verify the following classes are present and accessible:
- `Com.Google.Android.Filament.Engine`
- `Com.Google.Android.Filament.Renderer`
- `Com.Google.Android.Filament.View`
- `Com.Google.Android.Filament.Scene`
- `Com.Google.Android.Filament.Camera`
- `Com.Google.Android.Filament.SwapChain`
- `Com.Google.Android.Filament.Android.UiHelper`

Note any build errors or warnings — these drive the cleanup work in TASK-002.

### Step 6: Verify JNI initialization API is available

In a test project or LINQPad, confirm the initialization call compiles:

```csharp
// Must be called once before any other Filament API
Com.Google.Android.Filament.Filament.Init();
```

## Acceptance Criteria

- [ ] `FilamentBinding.Android.csproj` exists and is a valid `AndroidBindingLibrary` project targeting `net10.0-android`
- [ ] All four AARs are present under `Jars/`
- [ ] `dotnet build` completes without errors (warnings are expected at this stage)
- [ ] Generated binding exposes `Engine`, `Renderer`, `View`, `Scene`, `Camera`, `SwapChain`, `Material`, `MaterialInstance`, `Texture`, `EntityManager`, `TransformManager`, `RenderableManager`
- [ ] `Com.Google.Android.Filament.Android.UiHelper` is accessible in the generated binding
- [ ] `Transforms/Metadata.xml` and `Additions/FilamentExtensions.cs` placeholder files exist
- [ ] Build output file `FilamentBinding.Android.dll` is produced

## Reference

- See `.github/skills/filament-android-binding/SKILL.md`
- See `.github/skills/filament-maui-project-structure/SKILL.md`
- Maven Central: `https://repo1.maven.org/maven2/com/google/android/filament/`
- Android binding docs: `https://learn.microsoft.com/en-us/dotnet/android/binding-libs/`
- Filament Android source: `android/filament-android/src/main/java/com/google/android/filament/`
