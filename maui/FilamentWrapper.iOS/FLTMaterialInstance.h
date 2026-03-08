#import <Foundation/Foundation.h>
#import <simd/simd.h>
@class FLTTexture;

@interface FLTMaterialInstance : NSObject
- (instancetype)initWithNative:(void *)native;
- (void *)nativeMaterialInstance;
- (void)setFloatParameter:(NSString *)name value:(float)value;
- (void)setFloat4Parameter:(NSString *)name value:(simd_float4)value;
- (void)setTextureParameter:(NSString *)name texture:(FLTTexture *)texture;
@end
