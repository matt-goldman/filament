#import "FLTRenderTarget.h"
#import "FLTTexture.h"
#import "FLTEngine+Internal.h"
#include <filament/RenderTarget.h>
#include <filament/Engine.h>
using namespace filament;

// Private initializer used only within this translation unit
@interface FLTRenderTarget ()
- (instancetype)initWithNativeRenderTarget:(RenderTarget *)renderTarget;
@end

static RenderTarget::AttachmentPoint mapAttachment(FLTAttachmentPoint pt) {
    switch (pt) {
        case FLTAttachmentPointColor0: return RenderTarget::AttachmentPoint::COLOR0;
        case FLTAttachmentPointColor1: return RenderTarget::AttachmentPoint::COLOR1;
        case FLTAttachmentPointColor2: return RenderTarget::AttachmentPoint::COLOR2;
        case FLTAttachmentPointColor3: return RenderTarget::AttachmentPoint::COLOR3;
        case FLTAttachmentPointDepth:  return RenderTarget::AttachmentPoint::DEPTH;
        default:                       return RenderTarget::AttachmentPoint::COLOR0;
    }
}

// ---- FLTRenderTargetBuilder ----

@interface FLTRenderTargetBuilder ()
@property (nonatomic, strong) NSMutableArray *textures;
@property (nonatomic, strong) NSMutableArray *mipLevels;
@property (nonatomic, strong) NSMutableArray *attachmentPoints;
@end

@implementation FLTRenderTargetBuilder

- (instancetype)init {
    self = [super init];
    if (self) {
        _textures = [NSMutableArray array];
        _mipLevels = [NSMutableArray array];
        _attachmentPoints = [NSMutableArray array];
    }
    return self;
}

- (FLTRenderTargetBuilder *)texture:(FLTTexture *)texture
                         attachment:(FLTAttachmentPoint)attachment {
    [_textures addObject:texture];
    [_attachmentPoints addObject:@(attachment)];
    return self;
}

- (FLTRenderTargetBuilder *)mipLevel:(uint8_t)level
                          attachment:(FLTAttachmentPoint)attachment {
    // Store mip level for the matching attachment. For simplicity, append in order.
    [_mipLevels addObject:@(level)];
    return self;
}

- (FLTRenderTarget *)buildWithEngine:(FLTEngine *)engine {
    RenderTarget::Builder builder;
    for (NSUInteger i = 0; i < _textures.count; i++) {
        FLTTexture *tex = _textures[i];
        FLTAttachmentPoint ap = (FLTAttachmentPoint)[_attachmentPoints[i] unsignedIntegerValue];
        builder.texture(mapAttachment(ap), (Texture *)[tex nativeTexture]);
        if (i < _mipLevels.count) {
            uint8_t mip = (uint8_t)[_mipLevels[i] unsignedIntValue];
            builder.mipLevel(mapAttachment(ap), mip);
        }
    }
    RenderTarget *rt = builder.build(*[engine nativeEngine]);
    return [[FLTRenderTarget alloc] initWithNativeRenderTarget:rt];
}
@end

// ---- FLTRenderTarget ----

@implementation FLTRenderTarget {
    RenderTarget *_renderTarget;
}

+ (FLTRenderTargetBuilder *)builder {
    return [[FLTRenderTargetBuilder alloc] init];
}

- (instancetype)initWithNativeRenderTarget:(RenderTarget *)renderTarget {
    self = [super init];
    if (self) {
        _renderTarget = renderTarget;
    }
    return self;
}

- (void *)nativeRenderTarget { return _renderTarget; }

@end
