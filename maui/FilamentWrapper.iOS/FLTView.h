#import <Foundation/Foundation.h>
@class FLTScene, FLTCamera, FLTEngine;

@interface FLTView : NSObject
- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine;
- (void *)nativeView;
- (void)setScene:(FLTScene *)scene;
- (void)setCamera:(FLTCamera *)camera;
- (void)setViewportLeft:(int)left bottom:(int)bottom width:(uint32_t)width height:(uint32_t)height;
- (void)setClearColorRed:(float)r green:(float)g blue:(float)b alpha:(float)a;
- (void)setPostProcessingEnabled:(BOOL)enabled;
@end
