#import <Foundation/Foundation.h>
@class FLTEngine;

typedef NS_ENUM(NSUInteger, FLTVertexAttribute) {
    FLTVertexAttributePosition     = 0,
    FLTVertexAttributeTangents     = 1,
    FLTVertexAttributeColor        = 2,
    FLTVertexAttributeUV0          = 3,
    FLTVertexAttributeUV1          = 4,
    FLTVertexAttributeBoneIndices  = 5,
    FLTVertexAttributeBoneWeights  = 6,
    FLTVertexAttributeCustom0      = 7,
    FLTVertexAttributeCustom1      = 8,
};

typedef NS_ENUM(NSUInteger, FLTVertexAttributeType) {
    FLTVertexAttributeTypeByte      = 0,
    FLTVertexAttributeTypeByte2     = 1,
    FLTVertexAttributeTypeByte3     = 2,
    FLTVertexAttributeTypeByte4     = 3,
    FLTVertexAttributeTypeUByte     = 4,
    FLTVertexAttributeTypeUByte2    = 5,
    FLTVertexAttributeTypeUByte3    = 6,
    FLTVertexAttributeTypeUByte4    = 7,
    FLTVertexAttributeTypeShort     = 8,
    FLTVertexAttributeTypeShort2    = 9,
    FLTVertexAttributeTypeShort3    = 10,
    FLTVertexAttributeTypeShort4    = 11,
    FLTVertexAttributeTypeUShort    = 12,
    FLTVertexAttributeTypeUShort2   = 13,
    FLTVertexAttributeTypeUShort3   = 14,
    FLTVertexAttributeTypeUShort4   = 15,
    FLTVertexAttributeTypeInt       = 16,
    FLTVertexAttributeTypeUInt      = 17,
    FLTVertexAttributeTypeFloat     = 18,
    FLTVertexAttributeTypeFloat2    = 19,
    FLTVertexAttributeTypeFloat3    = 20,
    FLTVertexAttributeTypeFloat4    = 21,
    FLTVertexAttributeTypeHalf      = 22,
    FLTVertexAttributeTypeHalf2     = 23,
    FLTVertexAttributeTypeHalf3     = 24,
    FLTVertexAttributeTypeHalf4     = 25,
};

@interface FLTVertexBufferBuilder : NSObject
- (FLTVertexBufferBuilder *)vertexCount:(uint32_t)count;
- (FLTVertexBufferBuilder *)bufferCount:(uint8_t)count;
- (FLTVertexBufferBuilder *)attribute:(FLTVertexAttribute)attribute
                          bufferIndex:(uint8_t)bufferIndex
                        attributeType:(FLTVertexAttributeType)attributeType
                           byteOffset:(uint32_t)byteOffset
                           byteStride:(uint8_t)byteStride;
- (FLTVertexBufferBuilder *)normalizedAttribute:(FLTVertexAttribute)attribute
                                    normalized:(BOOL)normalized;
- (FLTVertexBuffer *)buildWithEngine:(FLTEngine *)engine;
@end

@interface FLTVertexBuffer : NSObject
+ (FLTVertexBufferBuilder *)builder;
- (void *)nativeVertexBuffer;
/// Upload vertex data. The contents of `data` are copied immediately and only need to remain
/// valid for the duration of this call.
- (void)setBufferAtIndex:(uint8_t)bufferIndex engine:(FLTEngine *)engine data:(NSData *)data;
@end
