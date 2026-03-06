// iOS platform implementations will be added in TASK-006 through TASK-009.
//
// The iOS binding requires an Objective-C++ wrapper framework (FilamentWrapper.xcframework)
// that does not yet exist. Once TASK-006 (FilamentWrapper.iOS) and TASK-007
// (FilamentBinding.iOS) are complete, the following files will be added here:
//
//   FilamentFactory.cs         — creates IFilamentEngine via FLTEngine
//   FilamentEngineIOS.cs       — IFilamentEngine wrapping FLTEngine
//   FilamentRendererIOS.cs     — IFilamentRenderer wrapping FLTRenderer
//   FilamentViewIOS.cs         — IFilamentView wrapping FLTView
//   FilamentSceneIOS.cs        — IFilamentScene wrapping FLTScene
//   FilamentCameraIOS.cs       — IFilamentCamera wrapping FLTCamera
//   FilamentSwapChainIOS.cs    — IFilamentSwapChain wrapping FLTSwapChain
//   FilamentMaterialIOS.cs     — IFilamentMaterial + IFilamentMaterialInstance wrapping FLT types
//   FilamentManagersIOS.cs     — IFilamentEntityManager, IFilamentTransformManager,
//                                IFilamentRenderableManager wrapping FLT managers
//   FilamentViewHandler.cs     — MAUI handler: UIView + CAMetalLayer + CADisplayLink
