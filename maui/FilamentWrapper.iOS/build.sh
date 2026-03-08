#!/usr/bin/env bash
# build.sh — Build FilamentWrapper.xcframework for iOS device and simulator
#
# Prerequisites:
#   • macOS with Xcode 15+
#   • Filament 1.69.5 iOS static libraries extracted from:
#     https://github.com/google/filament/releases/download/v1.69.5/filament-v1.69.5-ios.tgz
#   • The extracted tgz must be placed at <repo-root>/third_party/filament-ios/
#     (or adjust FILAMENT_IOS_DIR below)
#
# Usage:
#   ./build.sh [--clean]

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
BUILD_DIR="${SCRIPT_DIR}/build"
OUTPUT_XCFRAMEWORK="${SCRIPT_DIR}/FilamentWrapper.xcframework"
MAUI_NATIVE_DIR="${REPO_ROOT}/Plugin.Maui.Filament/FilamentBinding.iOS/Native"

# ---- Clean ----
if [[ "${1:-}" == "--clean" ]]; then
    echo "Cleaning build artifacts..."
    rm -rf "${BUILD_DIR}" "${OUTPUT_XCFRAMEWORK}" "${MAUI_NATIVE_DIR}/FilamentWrapper.xcframework"
    echo "Clean complete."
    exit 0
fi

mkdir -p "${BUILD_DIR}"

# Pipe to xcpretty only when it is available; otherwise let xcodebuild output flow directly.
# Either way the exit code is preserved (no || true masking).
_xcodebuild() {
    if command -v xcpretty &>/dev/null; then
        set -o pipefail
        xcodebuild "$@" | xcpretty
    else
        xcodebuild "$@"
    fi
}

echo "=== Building FilamentWrapper for iOS Device (arm64) ==="
_xcodebuild archive \
    -scheme FilamentWrapper \
    -project "${SCRIPT_DIR}/FilamentWrapper.xcodeproj" \
    -destination "generic/platform=iOS" \
    -archivePath "${BUILD_DIR}/ios.xcarchive" \
    SKIP_INSTALL=NO \
    BUILD_LIBRARY_FOR_DISTRIBUTION=YES

echo ""
echo "=== Building FilamentWrapper for iOS Simulator ==="
# Note: arm64 is excluded for the simulator slice per Filament's CocoaPods spec.
# The resulting simulator slice supports x86_64 (Intel Macs) only.
# On Apple Silicon Macs, use a physical iOS device or an Intel Mac for simulation.
_xcodebuild archive \
    -scheme FilamentWrapper \
    -project "${SCRIPT_DIR}/FilamentWrapper.xcodeproj" \
    -destination "generic/platform=iOS Simulator" \
    -archivePath "${BUILD_DIR}/ios-sim.xcarchive" \
    SKIP_INSTALL=NO \
    BUILD_LIBRARY_FOR_DISTRIBUTION=YES \
    EXCLUDED_ARCHS="arm64"

echo ""
echo "=== Creating FilamentWrapper.xcframework ==="
rm -rf "${OUTPUT_XCFRAMEWORK}"
xcodebuild -create-xcframework \
    -framework "${BUILD_DIR}/ios.xcarchive/Products/Library/Frameworks/FilamentWrapper.framework" \
    -framework "${BUILD_DIR}/ios-sim.xcarchive/Products/Library/Frameworks/FilamentWrapper.framework" \
    -output "${OUTPUT_XCFRAMEWORK}"

echo ""
echo "=== Copying to FilamentBinding.iOS/Native/ ==="
mkdir -p "${MAUI_NATIVE_DIR}"
rm -rf "${MAUI_NATIVE_DIR}/FilamentWrapper.xcframework"
cp -R "${OUTPUT_XCFRAMEWORK}" "${MAUI_NATIVE_DIR}/"

echo ""
echo "✅ Build complete: ${OUTPUT_XCFRAMEWORK}"
echo "   Copied to: ${MAUI_NATIVE_DIR}/FilamentWrapper.xcframework"
