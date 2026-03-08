#import "FLTRenderableManager.h"
#import "FLTEngine+Internal.h"
#import "FLTVertexBuffer.h"
#import "FLTIndexBuffer.h"
#import "FLTMaterialInstance.h"
#include <filament/RenderableManager.h>
#include <filament/VertexBuffer.h>
#include <filament/IndexBuffer.h>
#include <filament/MaterialInstance.h>
#include <filament/Box.h>
#include <utils/Entity.h>
using namespace filament;

// ---- FLTRenderableManagerBuilder ----

@interface FLTRenderableManagerBuilderEntry : NSObject
@property (nonatomic, assign) NSInteger index;
@property (nonatomic, assign) FLTPrimitiveType primitiveType;
@property (nonatomic, strong) FLTVertexBuffer *vertexBuffer;
@property (nonatomic, strong) FLTIndexBuffer *indexBuffer;
@property (nonatomic, strong) FLTMaterialInstance *materialInstance;
@end

@implementation FLTRenderableManagerBuilderEntry
@end

@interface FLTRenderableManagerBuilder ()
@property (nonatomic, assign) NSInteger count;
@property (nonatomic, strong) NSMutableArray<FLTRenderableManagerBuilderEntry *> *entries;
@property (nonatomic, assign) float bbCX, bbCY, bbCZ;
@property (nonatomic, assign) float bbHX, bbHY, bbHZ;
@property (nonatomic, assign) BOOL hasBoundingBox;
@property (nonatomic, assign) BOOL castShadowsFlag;
@property (nonatomic, assign) BOOL receiveShadowsFlag;
@end

@implementation FLTRenderableManagerBuilder

- (instancetype)initWithCount:(NSInteger)count {
    self = [super init];
    if (self) {
        _count = count;
        _entries = [NSMutableArray array];
        _castShadowsFlag = YES;
        _receiveShadowsFlag = YES;
    }
    return self;
}

- (FLTRenderableManagerBuilder *)geometryAtIndex:(NSInteger)index
                                   primitiveType:(FLTPrimitiveType)type
                                    vertexBuffer:(FLTVertexBuffer *)vb
                                     indexBuffer:(FLTIndexBuffer *)ib {
    FLTRenderableManagerBuilderEntry *e = [[FLTRenderableManagerBuilderEntry alloc] init];
    e.index = index;
    e.primitiveType = type;
    e.vertexBuffer = vb;
    e.indexBuffer = ib;
    [_entries addObject:e];
    return self;
}

- (FLTRenderableManagerBuilder *)materialAtIndex:(NSInteger)index
                                materialInstance:(FLTMaterialInstance *)mi {
    for (FLTRenderableManagerBuilderEntry *e in _entries) {
        if (e.index == index) {
            e.materialInstance = mi;
            return self;
        }
    }
    // If no geometry entry yet for this index, create a placeholder entry.
    FLTRenderableManagerBuilderEntry *e = [[FLTRenderableManagerBuilderEntry alloc] init];
    e.index = index;
    e.materialInstance = mi;
    [_entries addObject:e];
    return self;
}

- (FLTRenderableManagerBuilder *)boundingBoxCenterX:(float)cx centerY:(float)cy centerZ:(float)cz
                                        halfExtentX:(float)hx halfExtentY:(float)hy halfExtentZ:(float)hz {
    _bbCX = cx; _bbCY = cy; _bbCZ = cz;
    _bbHX = hx; _bbHY = hy; _bbHZ = hz;
    _hasBoundingBox = YES;
    return self;
}

- (FLTRenderableManagerBuilder *)castShadows:(BOOL)enable {
    _castShadowsFlag = enable;
    return self;
}

- (FLTRenderableManagerBuilder *)receiveShadows:(BOOL)enable {
    _receiveShadowsFlag = enable;
    return self;
}

- (void)buildWithEngine:(FLTEngine *)engine entity:(uint32_t)entity {
    RenderableManager::Builder builder((size_t)_count);

    for (FLTRenderableManagerBuilderEntry *e in _entries) {
        if (e.vertexBuffer && e.indexBuffer) {
            builder.geometry(
                (size_t)e.index,
                (RenderableManager::PrimitiveType)e.primitiveType,
                (VertexBuffer *)[e.vertexBuffer nativeVertexBuffer],
                (IndexBuffer *)[e.indexBuffer nativeIndexBuffer]
            );
        }
        if (e.materialInstance) {
            builder.material(
                (size_t)e.index,
                (MaterialInstance *)[e.materialInstance nativeMaterialInstance]
            );
        }
    }

    if (_hasBoundingBox) {
        Box bbox;
        bbox.center = {_bbCX, _bbCY, _bbCZ};
        bbox.halfExtent = {_bbHX, _bbHY, _bbHZ};
        builder.boundingBox(bbox);
    }

    builder.castShadows(_castShadowsFlag);
    builder.receiveShadows(_receiveShadowsFlag);
    builder.build(*[engine nativeEngine], utils::Entity::import(entity));
}
@end

// ---- FLTRenderableManager ----

@implementation FLTRenderableManager {
    RenderableManager *_mgr;
}

- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine {
    self = [super init];
    if (self) {
        _mgr = (RenderableManager *)native;
    }
    return self;
}

- (FLTRenderableManagerBuilder *)builderWithCount:(NSInteger)count {
    return [[FLTRenderableManagerBuilder alloc] initWithCount:count];
}

- (void)destroyComponent:(uint32_t)entity {
    _mgr->destroy(utils::Entity::import(entity));
}

@end
