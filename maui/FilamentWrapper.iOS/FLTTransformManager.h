#import <Foundation/Foundation.h>
#import <simd/simd.h>
@class FLTEngine;

@interface FLTTransformManager : NSObject
- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine;
- (void)createComponent:(uint32_t)entity;
- (void)destroyComponent:(uint32_t)entity;
- (void)setTransform:(simd_float4x4)transform forEntity:(uint32_t)entity;
- (simd_float4x4)getTransformForEntity:(uint32_t)entity;
@end
