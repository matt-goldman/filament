#import <Foundation/Foundation.h>
@class FLTEngine;

@interface FLTCamera : NSObject
- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine;
- (void *)nativeCamera;
- (void)setProjectionFov:(double)fovDegrees aspect:(double)aspect near:(double)near far:(double)far;
- (void)lookAtEyeX:(double)ex eyeY:(double)ey eyeZ:(double)ez
           centerX:(double)cx centerY:(double)cy centerZ:(double)cz
                upX:(double)ux upY:(double)uy upZ:(double)uz;
@end
