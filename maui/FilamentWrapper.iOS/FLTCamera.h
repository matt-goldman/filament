#import <Foundation/Foundation.h>
@class FLTEngine;

@interface FLTCamera : NSObject
- (instancetype)initWithNative:(void *)native entity:(uint32_t)entity engine:(FLTEngine *)engine;
- (void *)nativeCamera;
/// The entity ID used to create this camera. Required by FLTEngine destroyCamera:.
- (uint32_t)entityId;
- (void)setProjectionFov:(double)fovDegrees aspect:(double)aspect near:(double)near far:(double)far;
- (void)lookAtEyeX:(double)ex eyeY:(double)ey eyeZ:(double)ez
           centerX:(double)cx centerY:(double)cy centerZ:(double)cz
                upX:(double)ux upY:(double)uy upZ:(double)uz;
@end
