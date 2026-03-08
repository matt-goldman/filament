#import "FLTLightManager.h"
#import "FLTEngine+Internal.h"
#include <filament/LightManager.h>
#include <math/vec3.h>
#include <utils/Entity.h>
using namespace filament;

// ---- FLTLightManagerBuilder ----

@interface FLTLightManagerBuilder ()
@property (nonatomic, assign) FLTLightType lightType;
@property (nonatomic, assign) simd_float3 colorValue;
@property (nonatomic, assign) float intensityValue;
@property (nonatomic, assign) simd_float3 directionValue;
@property (nonatomic, assign) simd_float3 positionValue;
@property (nonatomic, assign) BOOL castShadowsFlag;
@property (nonatomic, assign) BOOL hasColor;
@property (nonatomic, assign) BOOL hasDirection;
@property (nonatomic, assign) BOOL hasPosition;
@end

@implementation FLTLightManagerBuilder

- (instancetype)initWithType:(FLTLightType)type {
    self = [super init];
    if (self) {
        _lightType = type;
        _intensityValue = 100000.0f;
    }
    return self;
}

- (FLTLightManagerBuilder *)color:(simd_float3)color {
    _colorValue = color;
    _hasColor = YES;
    return self;
}

- (FLTLightManagerBuilder *)intensity:(float)intensity {
    _intensityValue = intensity;
    return self;
}

- (FLTLightManagerBuilder *)direction:(simd_float3)direction {
    _directionValue = direction;
    _hasDirection = YES;
    return self;
}

- (FLTLightManagerBuilder *)position:(simd_float3)position {
    _positionValue = position;
    _hasPosition = YES;
    return self;
}

- (FLTLightManagerBuilder *)castShadows:(BOOL)enable {
    _castShadowsFlag = enable;
    return self;
}

- (void)buildWithEngine:(FLTEngine *)engine entity:(uint32_t)entity {
    LightManager::Builder builder((LightManager::Type)_lightType);

    if (_hasColor) {
        builder.color({_colorValue.x, _colorValue.y, _colorValue.z});
    }
    builder.intensity(_intensityValue);

    if (_hasDirection) {
        builder.direction({_directionValue.x, _directionValue.y, _directionValue.z});
    }
    if (_hasPosition) {
        builder.position({_positionValue.x, _positionValue.y, _positionValue.z});
    }
    builder.castShadows(_castShadowsFlag);
    builder.build(*[engine nativeEngine], utils::Entity::import(entity));
}
@end

// ---- FLTLightManager ----

@implementation FLTLightManager {
    LightManager *_mgr;
}

- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine {
    self = [super init];
    if (self) {
        _mgr = (LightManager *)native;
    }
    return self;
}

- (FLTLightManagerBuilder *)builderWithType:(FLTLightType)type {
    return [[FLTLightManagerBuilder alloc] initWithType:type];
}

- (void)destroyComponent:(uint32_t)entity {
    _mgr->destroy(utils::Entity::import(entity));
}

@end
