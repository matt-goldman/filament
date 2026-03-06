# TASK-011: NuGet Packaging and Distribution

**Phase:** 4 — Integration and Validation
**Estimated Effort:** 1–2 days
**Depends On:** TASK-005, TASK-009, TASK-010
**Relevant Skills:** `filament-maui-project-structure`

## Objective

Package the three .NET projects (`FilamentBinding.Android`, `FilamentBinding.iOS`, `Filament.Maui`) as NuGet packages suitable for distribution. Version packages to match the wrapped Filament version (1.69.5.x). Document how consumers install and use the packages in a MAUI app, and establish a strategy for handling the large iOS binary artifacts.

## Prerequisites

- TASK-005 complete — Android `FilamentViewHandler` working
- TASK-009 complete — iOS `FilamentViewHandler` working
- TASK-010 complete — sample app validates end-to-end integration
- `dotnet pack` available (included in .NET SDK)
- (Optional) NuGet account or private feed for publishing

## Deliverables

- NuGet package metadata added to all three `.csproj` files (`PackageId`, `Version`, `Description`, etc.)
- `maui/FilamentBinding.Android/FilamentBinding.Android.csproj` — pack-ready
- `maui/FilamentBinding.iOS/FilamentBinding.iOS.csproj` — pack-ready
- `maui/Filament.Maui/Filament.Maui.csproj` — pack-ready with dependency declarations
- `maui/pack.sh` — shell script that builds and packs all three packages in dependency order
- `maui/README-nuget.md` — consumer getting-started guide
- Three `.nupkg` files produced: `Filament.Maui.Binding.Android.1.69.5.x.nupkg`, `Filament.Maui.Binding.iOS.1.69.5.x.nupkg`, `Filament.Maui.1.69.5.x.nupkg`

## Detailed Steps

### Step 1: Add package metadata to FilamentBinding.Android

Edit `maui/FilamentBinding.Android/FilamentBinding.Android.csproj`:

```xml
<PropertyGroup>
  <TargetFramework>net10.0-android</TargetFramework>
  <Nullable>enable</Nullable>

  <!-- NuGet package metadata -->
  <PackageId>Filament.Maui.Binding.Android</PackageId>
  <Version>1.69.5.1</Version>
  <Authors>Your Name or Organization</Authors>
  <Description>
    .NET MAUI Android binding for the Filament 3D rendering engine v1.69.5.
    Wraps the official com.google.android.filament AAR artifacts.
  </Description>
  <PackageTags>filament;android;maui;3d;rendering;opengl;vulkan</PackageTags>
  <PackageProjectUrl>https://github.com/google/filament</PackageProjectUrl>
  <PackageLicenseExpression>Apache-2.0</PackageLicenseExpression>
  <RepositoryUrl>https://github.com/your-org/filament-maui</RepositoryUrl>
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
</PropertyGroup>
```

Version convention: `1.69.5.x` where `x` is the binding patch version.

### Step 2: Add package metadata to FilamentBinding.iOS

Edit `maui/FilamentBinding.iOS/FilamentBinding.iOS.csproj`:

```xml
<PropertyGroup>
  <TargetFramework>net10.0-ios</TargetFramework>
  <Nullable>enable</Nullable>

  <PackageId>Filament.Maui.Binding.iOS</PackageId>
  <Version>1.69.5.1</Version>
  <Authors>Your Name or Organization</Authors>
  <Description>
    .NET MAUI iOS binding for the Filament 3D rendering engine v1.69.5.
    Includes the FilamentWrapper.xcframework Objective-C++ wrapper.
  </Description>
  <PackageTags>filament;ios;maui;3d;rendering;metal</PackageTags>
  <PackageProjectUrl>https://github.com/google/filament</PackageProjectUrl>
  <PackageLicenseExpression>Apache-2.0</PackageLicenseExpression>
</PropertyGroup>
```

**iOS Binary Size Note:** The `FilamentWrapper.xcframework` includes statically linked Filament libraries totalling ~100 MB+ for all subspecs. To keep the NuGet package under the 250 MB limit:
- Include only the required subspecs: `filament`, `utils`, `backend`
- Skip `viewer`, `gltfio_core`, `camutils`, `filamat` unless the consumer explicitly needs them
- Consider Git LFS or a download script if the framework still exceeds the limit

### Step 3: Add package metadata to Filament.Maui

Edit `maui/Filament.Maui/Filament.Maui.csproj`:

```xml
<PropertyGroup>
  <TargetFrameworks>net10.0-android;net10.0-ios</TargetFrameworks>
  <Nullable>enable</Nullable>
  <UseMaui>true</UseMaui>

  <PackageId>Filament.Maui</PackageId>
  <Version>1.69.5.1</Version>
  <Authors>Your Name or Organization</Authors>
  <Description>
    Cross-platform .NET MAUI class library for the Filament 3D rendering engine v1.69.5.
    Provides IFilamentEngine, FilamentView, and platform handlers for Android and iOS.
  </Description>
  <PackageTags>filament;maui;3d;rendering;android;ios;cross-platform</PackageTags>
  <PackageProjectUrl>https://github.com/google/filament</PackageProjectUrl>
  <PackageLicenseExpression>Apache-2.0</PackageLicenseExpression>
</PropertyGroup>

<!-- Declare NuGet dependencies on the binding packages -->
<ItemGroup Condition="$(TargetFramework.StartsWith('net10.0-android'))">
  <!-- Replace ProjectReference with PackageReference when consuming from NuGet -->
  <ProjectReference Include="..\FilamentBinding.Android\FilamentBinding.Android.csproj" />
</ItemGroup>
<ItemGroup Condition="$(TargetFramework.StartsWith('net10.0-ios'))">
  <ProjectReference Include="..\FilamentBinding.iOS\FilamentBinding.iOS.csproj" />
</ItemGroup>
```

### Step 4: Create the pack script

`maui/pack.sh`:

```bash
#!/usr/bin/env bash
set -euo pipefail

VERSION="${1:-1.69.5.1}"
OUTPUT="./nupkgs"
mkdir -p "$OUTPUT"

echo "==> Packing Filament.Maui.Binding.Android $VERSION"
dotnet pack FilamentBinding.Android/FilamentBinding.Android.csproj \
  -c Release \
  -p:Version="$VERSION" \
  --output "$OUTPUT"

echo "==> Packing Filament.Maui.Binding.iOS $VERSION"
dotnet pack FilamentBinding.iOS/FilamentBinding.iOS.csproj \
  -c Release \
  -p:Version="$VERSION" \
  --output "$OUTPUT"

echo "==> Packing Filament.Maui $VERSION"
dotnet pack Filament.Maui/Filament.Maui.csproj \
  -c Release \
  -p:Version="$VERSION" \
  --output "$OUTPUT"

echo "==> Done. Packages written to $OUTPUT"
ls -lh "$OUTPUT"
```

Run with:
```bash
chmod +x maui/pack.sh
cd maui && ./pack.sh 1.69.5.1
```

### Step 5: Validate the packages locally

Before publishing, test the packages using a local NuGet source:

```bash
# Add local feed
dotnet nuget add source ./nupkgs --name FilamentLocal

# In the sample app project, switch from ProjectReference to PackageReference:
# <PackageReference Include="Filament.Maui" Version="1.69.5.1" />

# Restore and build
dotnet restore maui/FilamentSample/FilamentSample.csproj
dotnet build maui/FilamentSample/FilamentSample.csproj -f net10.0-android
```

### Step 6: Publish (when ready)

```bash
# Publish to NuGet.org
dotnet nuget push nupkgs/Filament.Maui.Binding.Android.1.69.5.1.nupkg \
  --api-key $NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json

dotnet nuget push nupkgs/Filament.Maui.Binding.iOS.1.69.5.1.nupkg \
  --api-key $NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json

dotnet nuget push nupkgs/Filament.Maui.1.69.5.1.nupkg \
  --api-key $NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

### Step 7: Author consumer documentation

Create `maui/README-nuget.md` covering:

1. Installation:
   ```xml
   <PackageReference Include="Filament.Maui" Version="1.69.5.1" />
   ```

2. Registration in `MauiProgram.cs`:
   ```csharp
   builder.UseFilament();
   ```

3. Usage in a page:
   ```xaml
   <filament:FilamentView x:Name="Surface" FrameRendering="OnFrame" />
   ```

4. Version mapping table:
   ```
   Filament.Maui 1.69.5.x  →  Filament native library 1.69.5
   ```

5. Platform requirements:
   - Android: API 21+, OpenGL ES 3.0 or Vulkan 1.0
   - iOS: 12.1+, Metal

## Acceptance Criteria

- [ ] All three `.csproj` files have complete NuGet package metadata (`PackageId`, `Version`, `Description`, `Authors`, `PackageLicenseExpression`)
- [ ] `dotnet pack` produces `Filament.Maui.Binding.Android.1.69.5.1.nupkg`
- [ ] `dotnet pack` produces `Filament.Maui.Binding.iOS.1.69.5.1.nupkg`
- [ ] `dotnet pack` produces `Filament.Maui.1.69.5.1.nupkg`
- [ ] `pack.sh` accepts a version argument and packs all three in dependency order
- [ ] `FilamentSample` builds successfully using local `PackageReference` instead of `ProjectReference`
- [ ] `README-nuget.md` documents installation, registration, and version mapping
- [ ] Version convention `1.69.5.x` is documented (Filament version + binding patch)
- [ ] iOS binary size strategy is documented (what's included, how to handle size limit)

## Reference

- See `.github/skills/filament-maui-project-structure/SKILL.md` — NuGet packaging strategy and binary distribution options
- NuGet package metadata: `https://learn.microsoft.com/en-us/nuget/reference/msbuild-targets#pack-target`
- .NET MAUI multi-targeted NuGet: `https://learn.microsoft.com/en-us/dotnet/maui/deployment/publish-nuget`
- Filament license (Apache 2.0): `https://github.com/google/filament/blob/main/LICENSE`
