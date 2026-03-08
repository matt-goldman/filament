#import <Foundation/Foundation.h>
@class FLTTexture, FLTEngine;

@interface FLTIndirectLightBuilder : NSObject
- (FLTIndirectLightBuilder *)reflections:(FLTTexture *)cubemap;
/// Set irradiance as spherical harmonics. bands must be 1, 2, or 3.
/// sh is an array of bands*bands simd_float3 coefficients.
- (FLTIndirectLightBuilder *)irradianceBands:(uint8_t)bands data:(NSData *)shData;
- (FLTIndirectLightBuilder *)intensity:(float)envIntensity;
- (FLTIndirectLight *)buildWithEngine:(FLTEngine *)engine;
@end

@interface FLTIndirectLight : NSObject
+ (FLTIndirectLightBuilder *)builder;
- (void *)nativeIndirectLight;
@end
