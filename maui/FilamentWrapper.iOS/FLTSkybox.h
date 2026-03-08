#import <Foundation/Foundation.h>
#import <simd/simd.h>
@class FLTTexture, FLTEngine;

@interface FLTSkyboxBuilder : NSObject
- (FLTSkyboxBuilder *)environment:(FLTTexture *)cubemap;
- (FLTSkyboxBuilder *)showSun:(BOOL)show;
- (FLTSkyboxBuilder *)color:(simd_float4)color;
- (FLTSkybox *)buildWithEngine:(FLTEngine *)engine;
@end

@interface FLTSkybox : NSObject
+ (FLTSkyboxBuilder *)builder;
- (void *)nativeSkybox;
@end
