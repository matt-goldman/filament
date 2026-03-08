#import "FLTIndexBuffer.h"
#import "FLTEngine+Internal.h"
#include <filament/IndexBuffer.h>
#include <filament/Engine.h>
using namespace filament;

// Private initializer used only within this translation unit
@interface FLTIndexBuffer ()
- (instancetype)initWithNativeIndexBuffer:(IndexBuffer *)ib;
@end

// ---- FLTIndexBufferBuilder ----

@interface FLTIndexBufferBuilder ()
@property (nonatomic, assign) uint32_t indexCountValue;
@property (nonatomic, assign) FLTIndexType indexTypeValue;
@end

@implementation FLTIndexBufferBuilder

- (instancetype)init {
    self = [super init];
    if (self) {
        _indexTypeValue = FLTIndexTypeUShort;
    }
    return self;
}

- (FLTIndexBufferBuilder *)indexCount:(uint32_t)count {
    _indexCountValue = count;
    return self;
}

- (FLTIndexBufferBuilder *)bufferType:(FLTIndexType)type {
    _indexTypeValue = type;
    return self;
}

- (FLTIndexBuffer *)buildWithEngine:(FLTEngine *)engine {
    IndexBuffer::IndexType nativeType = (_indexTypeValue == FLTIndexTypeUInt)
        ? IndexBuffer::IndexType::UINT
        : IndexBuffer::IndexType::USHORT;

    IndexBuffer *ib = IndexBuffer::Builder()
        .indexCount(_indexCountValue)
        .bufferType(nativeType)
        .build(*[engine nativeEngine]);

    FLTIndexBuffer *w = [[FLTIndexBuffer alloc] initWithNativeIndexBuffer:ib];
    return w;
}
@end

// ---- FLTIndexBuffer ----

@implementation FLTIndexBuffer {
    IndexBuffer *_indexBuffer;
}

+ (FLTIndexBufferBuilder *)builder {
    return [[FLTIndexBufferBuilder alloc] init];
}

- (instancetype)initWithNativeIndexBuffer:(IndexBuffer *)ib {
    self = [super init];
    if (self) {
        _indexBuffer = ib;
    }
    return self;
}

- (void *)nativeIndexBuffer { return _indexBuffer; }

- (void)setBufferWithEngine:(FLTEngine *)engine data:(NSData *)data {
    size_t dataSize = data.length;
    void *copy = malloc(dataSize);
    memcpy(copy, data.bytes, dataSize);

    IndexBuffer::BufferDescriptor buffer(
        copy, dataSize,
        [](void *buf, size_t, void *) { free(buf); }, nullptr
    );
    _indexBuffer->setBuffer(*[engine nativeEngine], std::move(buffer));
}

@end
