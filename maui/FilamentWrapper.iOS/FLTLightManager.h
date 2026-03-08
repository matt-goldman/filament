#import <Foundation/Foundation.h>
#import <simd/simd.h>
@class FLTEngine;

typedef NS_ENUM(NSUInteger, FLTLightType) {
    FLTLightTypeSun         = 0,
    FLTLightTypeDirectional = 1,
    FLTLightTypePoint       = 2,
    FLTLightTypeFocusedSpot = 3,
    FLTLightTypeSpot        = 4,
};

@interface FLTLightManagerBuilder : NSObject
- (instancetype)initWithType:(FLTLightType)type;
- (FLTLightManagerBuilder *)color:(simd_float3)color;
- (FLTLightManagerBuilder *)intensity:(float)intensity;
- (FLTLightManagerBuilder *)direction:(simd_float3)direction;
- (FLTLightManagerBuilder *)castShadows:(BOOL)enable;
- (FLTLightManagerBuilder *)position:(simd_float3)position;
- (void)buildWithEngine:(FLTEngine *)engine entity:(uint32_t)entity;
@end

@interface FLTLightManager : NSObject
- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine;
- (FLTLightManagerBuilder *)builderWithType:(FLTLightType)type;
- (void)destroyComponent:(uint32_t)entity;
@end
