#import "FLTTexture.h"
#import "FLTEngine+Internal.h"
#include <filament/Texture.h>
#include <filament/Engine.h>
using namespace filament;

// Private initializer used only within this translation unit
@interface FLTTexture ()
- (instancetype)initWithNativeTexture:(Texture *)texture;
@end

static Texture::InternalFormat mapTextureFormat(FLTTextureFormat fmt) {
    switch (fmt) {
        case FLTTextureFormatRGBA8:   return Texture::InternalFormat::RGBA8;
        case FLTTextureFormatRGB8:    return Texture::InternalFormat::RGB8;
        case FLTTextureFormatRGBA16F: return Texture::InternalFormat::RGBA16F;
        case FLTTextureFormatRGB16F:  return Texture::InternalFormat::RGB16F;
        case FLTTextureFormatR8:      return Texture::InternalFormat::R8;
        case FLTTextureFormatDEPTH32F: return Texture::InternalFormat::DEPTH32F;
        default:                      return Texture::InternalFormat::RGBA8;
    }
}

static Texture::Sampler mapSamplerType(FLTTextureSamplerType s) {
    switch (s) {
        case FLTTextureSamplerType2D:      return Texture::Sampler::SAMPLER_2D;
        case FLTTextureSamplerTypeCubeMap: return Texture::Sampler::SAMPLER_CUBEMAP;
        case FLTTextureSamplerType2DArray: return Texture::Sampler::SAMPLER_2D_ARRAY;
        default:                           return Texture::Sampler::SAMPLER_2D;
    }
}

// ---- FLTTextureBuilder ----

@interface FLTTextureBuilder ()
@property (nonatomic, assign) uint32_t w;
@property (nonatomic, assign) uint32_t h;
@property (nonatomic, assign) uint32_t d;
@property (nonatomic, assign) uint8_t  lvls;
@property (nonatomic, assign) FLTTextureSamplerType samplerType;
@property (nonatomic, assign) FLTTextureFormat fmt;
@property (nonatomic, assign) FLTTextureUsage use;
@end

@implementation FLTTextureBuilder

- (instancetype)init {
    self = [super init];
    if (self) {
        _w = 1; _h = 1; _d = 1; _lvls = 1;
        _samplerType = FLTTextureSamplerType2D;
        _fmt = FLTTextureFormatRGBA8;
        _use = FLTTextureUsageDefault;
    }
    return self;
}

- (FLTTextureBuilder *)width:(uint32_t)width   { _w = width; return self; }
- (FLTTextureBuilder *)height:(uint32_t)height  { _h = height; return self; }
- (FLTTextureBuilder *)depth:(uint32_t)depth    { _d = depth; return self; }
- (FLTTextureBuilder *)levels:(uint8_t)levels   { _lvls = levels; return self; }

- (FLTTextureBuilder *)sampler:(FLTTextureSamplerType)samplerType {
    _samplerType = samplerType; return self;
}
- (FLTTextureBuilder *)format:(FLTTextureFormat)format { _fmt = format; return self; }
- (FLTTextureBuilder *)usage:(FLTTextureUsage)usage    { _use = usage; return self; }

- (FLTTexture *)buildWithEngine:(FLTEngine *)engine {
    Texture *tex = Texture::Builder()
        .width(_w)
        .height(_h)
        .depth(_d)
        .levels(_lvls)
        .sampler(mapSamplerType(_samplerType))
        .format(mapTextureFormat(_fmt))
        .usage((Texture::Usage)_use)
        .build(*[engine nativeEngine]);
    return [[FLTTexture alloc] initWithNativeTexture:tex];
}
@end

// ---- FLTTexture ----

@implementation FLTTexture {
    Texture *_texture;
}

+ (FLTTextureBuilder *)builder {
    return [[FLTTextureBuilder alloc] init];
}

- (instancetype)initWithNativeTexture:(Texture *)texture {
    self = [super init];
    if (self) {
        _texture = texture;
    }
    return self;
}

- (void *)nativeTexture { return _texture; }

- (void)setImage:(FLTEngine *)engine level:(NSUInteger)level data:(NSData *)data {
    // Copy data into a heap buffer that Filament frees via the callback when the GPU is done.
    // The NSData argument can be released by the caller immediately after this call returns.
    size_t dataSize = data.length;
    void *copy = malloc(dataSize);
    memcpy(copy, data.bytes, dataSize);

    Texture::PixelBufferDescriptor buffer(
        copy, dataSize,
        Texture::Format::RGBA, Texture::Type::UBYTE,
        [](void *buf, size_t, void *) { free(buf); }, nullptr
    );
    _texture->setImage(*[engine nativeEngine], (size_t)level, std::move(buffer));
}

@end
