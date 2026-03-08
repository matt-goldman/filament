#import <Foundation/Foundation.h>
@class FLTEngine;

typedef NS_ENUM(NSUInteger, FLTIndexType) {
    FLTIndexTypeUShort = 0,
    FLTIndexTypeUInt   = 1,
};

@interface FLTIndexBufferBuilder : NSObject
- (FLTIndexBufferBuilder *)indexCount:(uint32_t)count;
- (FLTIndexBufferBuilder *)bufferType:(FLTIndexType)type;
- (FLTIndexBuffer *)buildWithEngine:(FLTEngine *)engine;
@end

@interface FLTIndexBuffer : NSObject
+ (FLTIndexBufferBuilder *)builder;
- (void *)nativeIndexBuffer;
/// Upload index data. Data must remain valid until the GPU consumes it (use EngineFlushAndWait).
- (void)setBufferWithEngine:(FLTEngine *)engine data:(NSData *)data;
@end
