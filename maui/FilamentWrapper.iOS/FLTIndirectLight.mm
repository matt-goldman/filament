#import "FLTIndirectLight.h"
#import "FLTTexture.h"
#import "FLTEngine+Internal.h"
#include <filament/IndirectLight.h>
#include <filament/Texture.h>
#include <math/vec3.h>
using namespace filament;

// Private initializer used only within this translation unit
@interface FLTIndirectLight ()
- (instancetype)initWithNativeIndirectLight:(filament::IndirectLight *)il;
@end

// ---- FLTIndirectLightBuilder ----

@interface FLTIndirectLightBuilder ()
@property (nonatomic, strong) FLTTexture *reflectionTexture;
@property (nonatomic, strong) NSData *shData;
@property (nonatomic, assign) uint8_t shBands;
@property (nonatomic, assign) float envIntensity;
@end

@implementation FLTIndirectLightBuilder

- (instancetype)init {
    self = [super init];
    if (self) {
        _envIntensity = 30000.0f;
        _shBands = 1;
    }
    return self;
}

- (FLTIndirectLightBuilder *)reflections:(FLTTexture *)cubemap {
    _reflectionTexture = cubemap;
    return self;
}

- (FLTIndirectLightBuilder *)irradianceBands:(uint8_t)bands data:(NSData *)shData {
    _shBands = bands;
    _shData = shData;
    return self;
}

- (FLTIndirectLightBuilder *)intensity:(float)envIntensity {
    _envIntensity = envIntensity;
    return self;
}

- (FLTIndirectLight *)buildWithEngine:(FLTEngine *)engine {
    IndirectLight::Builder builder;

    if (_reflectionTexture) {
        builder.reflections((Texture *)[_reflectionTexture nativeTexture]);
    }
    if (_shData) {
        // shData must contain bands*bands math::float3 values
        size_t expectedSize = (size_t)_shBands * (size_t)_shBands * sizeof(math::float3);
        if (_shData.length >= expectedSize) {
            builder.irradiance(_shBands, (const math::float3 *)_shData.bytes);
        }
    }
    builder.intensity(_envIntensity);

    filament::IndirectLight *il = builder.build(*[engine nativeEngine]);
    return [[FLTIndirectLight alloc] initWithNativeIndirectLight:il];
}
@end

// ---- FLTIndirectLight ----

@implementation FLTIndirectLight {
    filament::IndirectLight *_indirectLight;
}

+ (FLTIndirectLightBuilder *)builder {
    return [[FLTIndirectLightBuilder alloc] init];
}

- (instancetype)initWithNativeIndirectLight:(filament::IndirectLight *)il {
    self = [super init];
    if (self) {
        _indirectLight = il;
    }
    return self;
}

- (void *)nativeIndirectLight { return _indirectLight; }

@end
