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
// Each entry holds the texture, attachment point, and optional mip level for one attachment.
@property (nonatomic, strong) NSMutableDictionary<NSNumber *, FLTTexture *> *texturesByAttachment;
@property (nonatomic, strong) NSMutableDictionary<NSNumber *, NSNumber *> *mipLevelsByAttachment;
@end

@implementation FLTRenderTargetBuilder

- (instancetype)init {
    self = [super init];
    if (self) {
        _texturesByAttachment = [NSMutableDictionary dictionary];
        _mipLevelsByAttachment = [NSMutableDictionary dictionary];
    }
    return self;
}

- (FLTRenderTargetBuilder *)texture:(FLTTexture *)texture
                         attachment:(FLTAttachmentPoint)attachment {
    _texturesByAttachment[@(attachment)] = texture;
    return self;
}

- (FLTRenderTargetBuilder *)mipLevel:(uint8_t)level
                          attachment:(FLTAttachmentPoint)attachment {
    _mipLevelsByAttachment[@(attachment)] = @(level);
    return self;
}

- (FLTRenderTarget *)buildWithEngine:(FLTEngine *)engine {
    RenderTarget::Builder builder;
    for (NSNumber *apKey in _texturesByAttachment) {
        FLTAttachmentPoint ap = (FLTAttachmentPoint)[apKey unsignedIntegerValue];
        FLTTexture *tex = _texturesByAttachment[apKey];
        builder.texture(mapAttachment(ap), (Texture *)[tex nativeTexture]);
        NSNumber *mipNum = _mipLevelsByAttachment[apKey];
        if (mipNum) {
            builder.mipLevel(mapAttachment(ap), (uint8_t)[mipNum unsignedCharValue]);
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
