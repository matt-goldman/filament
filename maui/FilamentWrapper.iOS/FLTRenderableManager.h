#import <Foundation/Foundation.h>
#import <simd/simd.h>
@class FLTEngine, FLTVertexBuffer, FLTIndexBuffer, FLTMaterialInstance;

typedef NS_ENUM(NSUInteger, FLTPrimitiveType) {
    FLTPrimitiveTypePoints    = 0,
    FLTPrimitiveTypeLines     = 1,
    FLTPrimitiveTypeLineStrip = 2,
    FLTPrimitiveTypeTriangles = 3,
    FLTPrimitiveTypeTriangleStrip = 4,
};

@interface FLTRenderableManagerBuilder : NSObject
- (instancetype)initWithCount:(NSInteger)count;
- (FLTRenderableManagerBuilder *)geometryAtIndex:(NSInteger)index
                                   primitiveType:(FLTPrimitiveType)type
                                    vertexBuffer:(FLTVertexBuffer *)vb
                                     indexBuffer:(FLTIndexBuffer *)ib;
- (FLTRenderableManagerBuilder *)materialAtIndex:(NSInteger)index
                                materialInstance:(FLTMaterialInstance *)mi;
- (FLTRenderableManagerBuilder *)boundingBoxCenterX:(float)cx centerY:(float)cy centerZ:(float)cz
                                        halfExtentX:(float)hx halfExtentY:(float)hy halfExtentZ:(float)hz;
- (FLTRenderableManagerBuilder *)castShadows:(BOOL)enable;
- (FLTRenderableManagerBuilder *)receiveShadows:(BOOL)enable;
- (void)buildWithEngine:(FLTEngine *)engine entity:(uint32_t)entity;
@end

@interface FLTRenderableManager : NSObject
- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine;
- (FLTRenderableManagerBuilder *)builderWithCount:(NSInteger)count;
- (void)destroyComponent:(uint32_t)entity;
@end
