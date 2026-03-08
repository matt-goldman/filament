#import <Foundation/Foundation.h>
@class FLTEngine;

@interface FLTSwapChain : NSObject
- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine;
- (void *)nativeSwapChain;
@end
