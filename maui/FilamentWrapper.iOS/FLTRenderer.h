#import <Foundation/Foundation.h>
@class FLTSwapChain, FLTView, FLTEngine;

@interface FLTRenderer : NSObject
- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine;
- (void *)nativeRenderer;
- (BOOL)beginFrame:(FLTSwapChain *)swapChain;
- (void)render:(FLTView *)view;
- (void)endFrame;
@end
