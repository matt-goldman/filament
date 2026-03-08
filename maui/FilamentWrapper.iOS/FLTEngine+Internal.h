// FLTEngine+Internal.h
// Internal header — NOT part of the public framework API.
// Import this in .mm files that need to access the underlying filament::Engine*.

#import "FLTEngine.h"
#include <filament/Engine.h>

@interface FLTEngine (Internal)
/// Returns the underlying filament::Engine pointer.
/// Must only be called from .mm implementation files within this framework.
- (filament::Engine *)nativeEngine;
@end
