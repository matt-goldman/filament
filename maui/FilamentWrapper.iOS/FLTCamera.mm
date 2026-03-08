#import "FLTCamera.h"
#include <filament/Camera.h>
#include <math/vec3.h>
using namespace filament;

@implementation FLTCamera {
    Camera *_camera;
}

- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine {
    self = [super init];
    if (self) {
        _camera = (Camera *)native;
    }
    return self;
}

- (void *)nativeCamera { return _camera; }

- (void)setProjectionFov:(double)fovDegrees aspect:(double)aspect near:(double)near far:(double)far {
    _camera->setProjection(fovDegrees, aspect, near, far, Camera::Fov::VERTICAL);
}

- (void)lookAtEyeX:(double)ex eyeY:(double)ey eyeZ:(double)ez
           centerX:(double)cx centerY:(double)cy centerZ:(double)cz
                upX:(double)ux upY:(double)uy upZ:(double)uz {
    // Convert scalar parameters to math::double3 for the lookAt call
    _camera->lookAt(
        {ex, ey, ez},
        {cx, cy, cz},
        {ux, uy, uz}
    );
}

@end
