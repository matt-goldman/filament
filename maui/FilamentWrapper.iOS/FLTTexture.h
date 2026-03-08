#import <Foundation/Foundation.h>
@class FLTEngine;

typedef NS_ENUM(NSUInteger, FLTTextureFormat) {
    FLTTextureFormatRGBA8  = 0,
    FLTTextureFormatRGB8   = 1,
    FLTTextureFormatRGBA16F = 2,
    FLTTextureFormatRGB16F = 3,
    FLTTextureFormatR8     = 4,
    FLTTextureFormatDEPTH32F = 5,
};

typedef NS_OPTIONS(NSUInteger, FLTTextureUsage) {
    FLTTextureUsageColorAttachment   = 0x1,
    FLTTextureUsageDepthAttachment   = 0x2,
    FLTTextureUsageStencilAttachment = 0x4,
    FLTTextureUsageUploadable        = 0x8,
    FLTTextureUsageSampleable        = 0x10,
    FLTTextureUsageDefault           = 0x18,
};

typedef NS_ENUM(NSUInteger, FLTTextureSamplerType) {
    FLTTextureSamplerType2D     = 0,
    FLTTextureSamplerTypeCubeMap = 1,
    FLTTextureSamplerType2DArray = 2,
};

@interface FLTTextureBuilder : NSObject
- (FLTTextureBuilder *)width:(uint32_t)width;
- (FLTTextureBuilder *)height:(uint32_t)height;
- (FLTTextureBuilder *)depth:(uint32_t)depth;
- (FLTTextureBuilder *)levels:(uint8_t)levels;
- (FLTTextureBuilder *)sampler:(FLTTextureSamplerType)samplerType;
- (FLTTextureBuilder *)format:(FLTTextureFormat)format;
- (FLTTextureBuilder *)usage:(FLTTextureUsage)usage;
- (FLTTexture *)buildWithEngine:(FLTEngine *)engine;
@end

@interface FLTTexture : NSObject
+ (FLTTextureBuilder *)builder;
- (void *)nativeTexture;
/// Upload pixel data to the specified mip level. The contents of `data` are copied immediately
/// and only need to remain valid for the duration of this call.
- (void)setImage:(FLTEngine *)engine level:(NSUInteger)level data:(NSData *)data;
@end
