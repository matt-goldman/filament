#import <Foundation/Foundation.h>
@class FLTSwapChain, FLTView, FLTEngine;

@interface FLTRenderer : NSObject
- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine;
- (void *)nativeRenderer;
- (BOOL)beginFrame:(FLTSwapChain *)swapChain;
- (void)render:(FLTView *)view;
- (void)endFrame;
/// Set the clear color applied at the start of each frame.
- (void)setClearColorRed:(float)r green:(float)g blue:(float)b alpha:(float)a;
@end
