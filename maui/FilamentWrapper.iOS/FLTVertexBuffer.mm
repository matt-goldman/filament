#import "FLTVertexBuffer.h"
#import "FLTEngine+Internal.h"
#include <filament/VertexBuffer.h>
#include <filament/Engine.h>
using namespace filament;

// Private initializer used only within this translation unit
@interface FLTVertexBuffer ()
- (instancetype)initWithNativeVertexBuffer:(VertexBuffer *)vb;
@end

static VertexAttribute mapVertexAttribute(FLTVertexAttribute attr) {
    switch (attr) {
        case FLTVertexAttributePosition:    return VertexAttribute::POSITION;
        case FLTVertexAttributeTangents:    return VertexAttribute::TANGENTS;
        case FLTVertexAttributeColor:       return VertexAttribute::COLOR;
        case FLTVertexAttributeUV0:         return VertexAttribute::UV0;
        case FLTVertexAttributeUV1:         return VertexAttribute::UV1;
        case FLTVertexAttributeBoneIndices: return VertexAttribute::BONE_INDICES;
        case FLTVertexAttributeBoneWeights: return VertexAttribute::BONE_WEIGHTS;
        case FLTVertexAttributeCustom0:     return VertexAttribute::CUSTOM0;
        case FLTVertexAttributeCustom1:     return VertexAttribute::CUSTOM1;
        default:                            return VertexAttribute::POSITION;
    }
}

static VertexBuffer::AttributeType mapAttributeType(FLTVertexAttributeType type) {
    switch (type) {
        case FLTVertexAttributeTypeByte:    return VertexBuffer::AttributeType::BYTE;
        case FLTVertexAttributeTypeByte2:   return VertexBuffer::AttributeType::BYTE2;
        case FLTVertexAttributeTypeByte3:   return VertexBuffer::AttributeType::BYTE3;
        case FLTVertexAttributeTypeByte4:   return VertexBuffer::AttributeType::BYTE4;
        case FLTVertexAttributeTypeUByte:   return VertexBuffer::AttributeType::UBYTE;
        case FLTVertexAttributeTypeUByte2:  return VertexBuffer::AttributeType::UBYTE2;
        case FLTVertexAttributeTypeUByte3:  return VertexBuffer::AttributeType::UBYTE3;
        case FLTVertexAttributeTypeUByte4:  return VertexBuffer::AttributeType::UBYTE4;
        case FLTVertexAttributeTypeShort:   return VertexBuffer::AttributeType::SHORT;
        case FLTVertexAttributeTypeShort2:  return VertexBuffer::AttributeType::SHORT2;
        case FLTVertexAttributeTypeShort3:  return VertexBuffer::AttributeType::SHORT3;
        case FLTVertexAttributeTypeShort4:  return VertexBuffer::AttributeType::SHORT4;
        case FLTVertexAttributeTypeUShort:  return VertexBuffer::AttributeType::USHORT;
        case FLTVertexAttributeTypeUShort2: return VertexBuffer::AttributeType::USHORT2;
        case FLTVertexAttributeTypeUShort3: return VertexBuffer::AttributeType::USHORT3;
        case FLTVertexAttributeTypeUShort4: return VertexBuffer::AttributeType::USHORT4;
        case FLTVertexAttributeTypeInt:     return VertexBuffer::AttributeType::INT;
        case FLTVertexAttributeTypeUInt:    return VertexBuffer::AttributeType::UINT;
        case FLTVertexAttributeTypeFloat:   return VertexBuffer::AttributeType::FLOAT;
        case FLTVertexAttributeTypeFloat2:  return VertexBuffer::AttributeType::FLOAT2;
        case FLTVertexAttributeTypeFloat3:  return VertexBuffer::AttributeType::FLOAT3;
        case FLTVertexAttributeTypeFloat4:  return VertexBuffer::AttributeType::FLOAT4;
        case FLTVertexAttributeTypeHalf:    return VertexBuffer::AttributeType::HALF;
        case FLTVertexAttributeTypeHalf2:   return VertexBuffer::AttributeType::HALF2;
        case FLTVertexAttributeTypeHalf3:   return VertexBuffer::AttributeType::HALF3;
        case FLTVertexAttributeTypeHalf4:   return VertexBuffer::AttributeType::HALF4;
        default:                            return VertexBuffer::AttributeType::FLOAT;
    }
}

// ---- FLTVertexBufferBuilder ----

@interface FLTVertexBufferAttributeEntry : NSObject
@property (nonatomic, assign) FLTVertexAttribute attribute;
@property (nonatomic, assign) uint8_t bufferIndex;
@property (nonatomic, assign) FLTVertexAttributeType attributeType;
@property (nonatomic, assign) uint32_t byteOffset;
@property (nonatomic, assign) uint8_t byteStride;
@property (nonatomic, assign) BOOL normalized;
@end

@implementation FLTVertexBufferAttributeEntry
@end

@interface FLTVertexBufferBuilder ()
@property (nonatomic, assign) uint32_t vertexCountValue;
@property (nonatomic, assign) uint8_t bufferCountValue;
@property (nonatomic, strong) NSMutableArray<FLTVertexBufferAttributeEntry *> *attributes;
@end

@implementation FLTVertexBufferBuilder

- (instancetype)init {
    self = [super init];
    if (self) {
        _attributes = [NSMutableArray array];
    }
    return self;
}

- (FLTVertexBufferBuilder *)vertexCount:(uint32_t)count {
    _vertexCountValue = count;
    return self;
}

- (FLTVertexBufferBuilder *)bufferCount:(uint8_t)count {
    _bufferCountValue = count;
    return self;
}

- (FLTVertexBufferBuilder *)attribute:(FLTVertexAttribute)attribute
                          bufferIndex:(uint8_t)bufferIndex
                        attributeType:(FLTVertexAttributeType)attributeType
                           byteOffset:(uint32_t)byteOffset
                           byteStride:(uint8_t)byteStride {
    FLTVertexBufferAttributeEntry *e = [[FLTVertexBufferAttributeEntry alloc] init];
    e.attribute = attribute;
    e.bufferIndex = bufferIndex;
    e.attributeType = attributeType;
    e.byteOffset = byteOffset;
    e.byteStride = byteStride;
    e.normalized = NO;
    [_attributes addObject:e];
    return self;
}

- (FLTVertexBufferBuilder *)normalizedAttribute:(FLTVertexAttribute)attribute
                                    normalized:(BOOL)normalized {
    for (FLTVertexBufferAttributeEntry *e in _attributes) {
        if (e.attribute == attribute) {
            e.normalized = normalized;
            return self;
        }
    }
    return self;
}

- (FLTVertexBuffer *)buildWithEngine:(FLTEngine *)engine {
    VertexBuffer::Builder builder;
    builder.vertexCount(_vertexCountValue);
    builder.bufferCount(_bufferCountValue);

    for (FLTVertexBufferAttributeEntry *e in _attributes) {
        builder.attribute(
            mapVertexAttribute(e.attribute),
            e.bufferIndex,
            mapAttributeType(e.attributeType),
            e.byteOffset,
            e.byteStride
        );
        if (e.normalized) {
            builder.normalized(mapVertexAttribute(e.attribute));
        }
    }

    VertexBuffer *vb = builder.build(*[engine nativeEngine]);
    return [[FLTVertexBuffer alloc] initWithNativeVertexBuffer:vb];
}
@end

// ---- FLTVertexBuffer ----

@implementation FLTVertexBuffer {
    VertexBuffer *_vertexBuffer;
}

+ (FLTVertexBufferBuilder *)builder {
    return [[FLTVertexBufferBuilder alloc] init];
}

- (instancetype)initWithNativeVertexBuffer:(VertexBuffer *)vb {
    self = [super init];
    if (self) {
        _vertexBuffer = vb;
    }
    return self;
}

- (void *)nativeVertexBuffer { return _vertexBuffer; }

- (void)setBufferAtIndex:(uint8_t)bufferIndex engine:(FLTEngine *)engine data:(NSData *)data {
    size_t dataSize = data.length;
    void *copy = malloc(dataSize);
    memcpy(copy, data.bytes, dataSize);

    VertexBuffer::BufferDescriptor buffer(
        copy, dataSize,
        [](void *buf, size_t, void *) { free(buf); }, nullptr
    );
    _vertexBuffer->setBufferAt(*[engine nativeEngine], bufferIndex, std::move(buffer));
}

@end
