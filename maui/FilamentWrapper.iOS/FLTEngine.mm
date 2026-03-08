#import "FLTEngine.h"
#import "FLTRenderer.h"
#import "FLTScene.h"
#import "FLTView.h"
#import "FLTCamera.h"
#import "FLTSwapChain.h"
#import "FLTTransformManager.h"
#import "FLTRenderableManager.h"
#import "FLTLightManager.h"
#import "FLTEntityManager.h"
#import "FLTTexture.h"
#import "FLTVertexBuffer.h"
#import "FLTIndexBuffer.h"
#import "FLTMaterial.h"
#import "FLTMaterialInstance.h"
#import "FLTRenderTarget.h"
#import "FLTIndirectLight.h"
#import "FLTSkybox.h"
#include <filament/Engine.h>
#include <filament/Texture.h>
#include <filament/VertexBuffer.h>
#include <filament/IndexBuffer.h>
#include <filament/Material.h>
#include <filament/MaterialInstance.h>
#include <filament/RenderTarget.h>
#include <filament/IndirectLight.h>
#include <filament/Skybox.h>
#include <utils/EntityManager.h>
using namespace filament;

@implementation FLTEngine {
    Engine *_engine;  // Raw C++ pointer; Engine::destroy(&_engine) zeroes it
}

+ (instancetype)createWithBackend:(FLTBackend)backend {
    FLTEngine *w = [[FLTEngine alloc] init];
    w->_engine = Engine::create((Engine::Backend)backend);
    return w;
}

- (void)destroy {
    // CRITICAL: Engine::destroy takes pointer-to-pointer and sets the pointer to null.
    // Do NOT pass _engine directly — pass &_engine.
    Engine::destroy(&_engine);
}

- (Engine *)nativeEngine { return _engine; }

- (FLTRenderer *)createRenderer {
    return [[FLTRenderer alloc] initWithNative:_engine->createRenderer() engine:self];
}

- (FLTScene *)createScene {
    return [[FLTScene alloc] initWithNative:_engine->createScene() engine:self];
}

- (FLTView *)createView {
    return [[FLTView alloc] initWithNative:_engine->createView() engine:self];
}

- (FLTCamera *)createCamera {
    utils::Entity camEntity = utils::EntityManager::get().create();
    Camera *cam = _engine->createCamera(camEntity);
    return [[FLTCamera alloc] initWithNative:cam entity:camEntity.getId() engine:self];
}

- (FLTSwapChain *)createSwapChainFromLayer:(void *)nativeLayer {
    // nativeLayer must be a CAMetalLayer configured with MTLPixelFormatBGRA8Unorm
    SwapChain *sc = _engine->createSwapChain(nativeLayer);
    return [[FLTSwapChain alloc] initWithNative:sc engine:self];
}

- (void)destroyRenderer:(FLTRenderer *)r   { _engine->destroy((Renderer *)[r nativeRenderer]); }
- (void)destroyScene:(FLTScene *)s         { _engine->destroy((Scene *)[s nativeScene]); }
- (void)destroyView:(FLTView *)v           { _engine->destroy((View *)[v nativeView]); }
- (void)destroySwapChain:(FLTSwapChain *)sc { _engine->destroy((SwapChain *)[sc nativeSwapChain]); }

- (void)destroyCamera:(FLTCamera *)c {
    // Cameras are components: use destroyCameraComponent(entity), then destroy the entity.
    utils::Entity entity = utils::Entity::import([c entityId]);
    _engine->destroyCameraComponent(entity);
    utils::EntityManager::get().destroy(entity);
}

- (void)destroyTexture:(FLTTexture *)texture {
    _engine->destroy((Texture *)[texture nativeTexture]);
}
- (void)destroyVertexBuffer:(FLTVertexBuffer *)vb {
    _engine->destroy((VertexBuffer *)[vb nativeVertexBuffer]);
}
- (void)destroyIndexBuffer:(FLTIndexBuffer *)ib {
    _engine->destroy((IndexBuffer *)[ib nativeIndexBuffer]);
}
- (void)destroyMaterial:(FLTMaterial *)mat {
    _engine->destroy((Material *)[mat nativeMaterial]);
}
- (void)destroyMaterialInstance:(FLTMaterialInstance *)mi {
    _engine->destroy((MaterialInstance *)[mi nativeMaterialInstance]);
}
- (void)destroyRenderTarget:(FLTRenderTarget *)rt {
    _engine->destroy((RenderTarget *)[rt nativeRenderTarget]);
}
- (void)destroyIndirectLight:(FLTIndirectLight *)il {
    _engine->destroy((IndirectLight *)[il nativeIndirectLight]);
}
- (void)destroySkybox:(FLTSkybox *)sky {
    _engine->destroy((Skybox *)[sky nativeSkybox]);
}

- (void)flushAndWait { _engine->flushAndWait(); }

- (FLTTransformManager *)transformManager {
    return [[FLTTransformManager alloc] initWithNative:&_engine->getTransformManager() engine:self];
}

- (FLTRenderableManager *)renderableManager {
    return [[FLTRenderableManager alloc] initWithNative:&_engine->getRenderableManager() engine:self];
}

- (FLTLightManager *)lightManager {
    return [[FLTLightManager alloc] initWithNative:&_engine->getLightManager() engine:self];
}

- (FLTEntityManager *)entityManager {
    return [[FLTEntityManager alloc] init];
}

@end
