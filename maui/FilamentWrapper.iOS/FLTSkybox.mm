#import "FLTSkybox.h"
#import "FLTTexture.h"
#import "FLTEngine+Internal.h"
#include <filament/Skybox.h>
#include <filament/Texture.h>
#include <math/vec4.h>
using namespace filament;

// Private initializer used only within this translation unit
@interface FLTSkybox ()
- (instancetype)initWithNativeSkybox:(Skybox *)skybox;
@end

// ---- FLTSkyboxBuilder ----

@interface FLTSkyboxBuilder ()
@property (nonatomic, strong) FLTTexture *environmentTexture;
@property (nonatomic, assign) BOOL showSunFlag;
@property (nonatomic, assign) BOOL hasColor;
@property (nonatomic, assign) simd_float4 colorValue;
@end

@implementation FLTSkyboxBuilder

- (FLTSkyboxBuilder *)environment:(FLTTexture *)cubemap {
    _environmentTexture = cubemap;
    return self;
}

- (FLTSkyboxBuilder *)showSun:(BOOL)show {
    _showSunFlag = show;
    return self;
}

- (FLTSkyboxBuilder *)color:(simd_float4)color {
    _colorValue = color;
    _hasColor = YES;
    return self;
}

- (FLTSkybox *)buildWithEngine:(FLTEngine *)engine {
    Skybox::Builder builder;

    if (_environmentTexture) {
        builder.environment((Texture *)[_environmentTexture nativeTexture]);
    }
    builder.showSun(_showSunFlag);
    if (_hasColor) {
        builder.color({_colorValue.x, _colorValue.y, _colorValue.z, _colorValue.w});
    }

    Skybox *sky = builder.build(*[engine nativeEngine]);
    return [[FLTSkybox alloc] initWithNativeSkybox:sky];
}
@end

// ---- FLTSkybox ----

@implementation FLTSkybox {
    Skybox *_skybox;
}

+ (FLTSkyboxBuilder *)builder {
    return [[FLTSkyboxBuilder alloc] init];
}

- (instancetype)initWithNativeSkybox:(Skybox *)skybox {
    self = [super init];
    if (self) {
        _skybox = skybox;
    }
    return self;
}

- (void *)nativeSkybox { return _skybox; }

@end
