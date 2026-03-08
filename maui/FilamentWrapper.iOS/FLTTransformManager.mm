#import "FLTTransformManager.h"
#import "FLTEngine.h"
#include <filament/TransformManager.h>
#include <math/mat4.h>
#include <utils/Entity.h>
using namespace filament;

@implementation FLTTransformManager {
    TransformManager *_mgr;
}

- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine {
    self = [super init];
    if (self) {
        _mgr = (TransformManager *)native;
    }
    return self;
}

- (void)createComponent:(uint32_t)entity {
    _mgr->create(utils::Entity::import(entity));
}

- (void)destroyComponent:(uint32_t)entity {
    auto i = _mgr->getInstance(utils::Entity::import(entity));
    _mgr->destroy(utils::Entity::import(entity));
}

- (void)setTransform:(simd_float4x4)m forEntity:(uint32_t)entity {
    // Convert simd_float4x4 to filament math::mat4f (column-major, same memory layout)
    math::mat4f mat(
        math::float4{m.columns[0].x, m.columns[0].y, m.columns[0].z, m.columns[0].w},
        math::float4{m.columns[1].x, m.columns[1].y, m.columns[1].z, m.columns[1].w},
        math::float4{m.columns[2].x, m.columns[2].y, m.columns[2].z, m.columns[2].w},
        math::float4{m.columns[3].x, m.columns[3].y, m.columns[3].z, m.columns[3].w}
    );
    auto i = _mgr->getInstance(utils::Entity::import(entity));
    _mgr->setTransform(i, mat);
}

- (simd_float4x4)getTransformForEntity:(uint32_t)entity {
    auto i = _mgr->getInstance(utils::Entity::import(entity));
    math::mat4f mat = _mgr->getTransform(i);
    simd_float4x4 result;
    for (int col = 0; col < 4; col++) {
        result.columns[col] = simd_make_float4(
            mat[col][0], mat[col][1], mat[col][2], mat[col][3]
        );
    }
    return result;
}

@end
