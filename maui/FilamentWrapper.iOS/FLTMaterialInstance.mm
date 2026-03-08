#import "FLTMaterialInstance.h"
#import "FLTTexture.h"
#include <filament/MaterialInstance.h>
#include <filament/Texture.h>
#include <filament/TextureSampler.h>
#include <math/vec4.h>
using namespace filament;

@implementation FLTMaterialInstance {
    MaterialInstance *_instance;
}

- (instancetype)initWithNative:(void *)native {
    self = [super init];
    if (self) {
        _instance = (MaterialInstance *)native;
    }
    return self;
}

- (void *)nativeMaterialInstance { return _instance; }

- (void)setFloatParameter:(NSString *)name value:(float)value {
    _instance->setParameter(name.UTF8String, value);
}

- (void)setFloat4Parameter:(NSString *)name value:(simd_float4)value {
    math::float4 v{value.x, value.y, value.z, value.w};
    _instance->setParameter(name.UTF8String, v);
}

- (void)setTextureParameter:(NSString *)name texture:(FLTTexture *)texture {
    TextureSampler sampler(TextureSampler::MinFilter::LINEAR, TextureSampler::MagFilter::LINEAR);
    _instance->setParameter(name.UTF8String, (Texture *)[texture nativeTexture], sampler);
}

@end
