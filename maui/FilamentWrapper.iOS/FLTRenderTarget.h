#import <Foundation/Foundation.h>
@class FLTTexture, FLTEngine;

typedef NS_ENUM(NSUInteger, FLTAttachmentPoint) {
    FLTAttachmentPointColor0  = 0,
    FLTAttachmentPointColor1  = 1,
    FLTAttachmentPointColor2  = 2,
    FLTAttachmentPointColor3  = 3,
    FLTAttachmentPointDepth   = 4,
};

@interface FLTRenderTargetBuilder : NSObject
- (FLTRenderTargetBuilder *)texture:(FLTTexture *)texture
                         attachment:(FLTAttachmentPoint)attachment;
- (FLTRenderTargetBuilder *)mipLevel:(uint8_t)level
                          attachment:(FLTAttachmentPoint)attachment;
- (FLTRenderTarget *)buildWithEngine:(FLTEngine *)engine;
@end

@interface FLTRenderTarget : NSObject
+ (FLTRenderTargetBuilder *)builder;
- (void *)nativeRenderTarget;
@end
