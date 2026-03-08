# FilamentBinding.iOS/Native

This directory holds the `FilamentWrapper.xcframework` binary that is referenced by
`FilamentBinding.iOS.csproj`.

## How to populate this directory

1. On a macOS machine with Xcode 15+ and the Filament 1.69.5 iOS static libraries:

   ```bash
   cd maui/FilamentWrapper.iOS
   ./build.sh
   ```

2. The build script automatically copies the resulting `FilamentWrapper.xcframework`
   to this directory.

## Manual steps (if needed)

If the static libraries are not yet extracted, download them first:

```bash
curl -L https://github.com/google/filament/releases/download/v1.69.5/filament-v1.69.5-ios.tgz \
  -o /tmp/filament-ios.tgz
mkdir -p third_party/filament-ios
tar -xzf /tmp/filament-ios.tgz -C third_party/filament-ios
```

Then run `./build.sh` from `maui/FilamentWrapper.iOS/`.

## What's included in the xcframework

The xcframework is built for two slices:
- `ios-arm64` — physical iOS devices
- `ios-x86_64-simulator` — iOS Simulator on Intel Macs (arm64 excluded per Filament spec; see build.sh)
