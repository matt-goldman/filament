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
/// Upload index data. The contents of `data` are copied immediately and only need to remain
/// valid for the duration of this call.
- (void)setBufferWithEngine:(FLTEngine *)engine data:(NSData *)data;
@end
