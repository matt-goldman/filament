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
#include <filament/Engine.h>
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
    return [[FLTCamera alloc] initWithNative:_engine->createCamera(camEntity) engine:self];
}

- (FLTSwapChain *)createSwapChainFromLayer:(void *)nativeLayer {
    // nativeLayer must be a CAMetalLayer configured with MTLPixelFormatBGRA8Unorm
    SwapChain *sc = _engine->createSwapChain(nativeLayer);
    return [[FLTSwapChain alloc] initWithNative:sc engine:self];
}

- (void)destroyRenderer:(FLTRenderer *)r  { _engine->destroy((Renderer *)[r nativeRenderer]); }
- (void)destroyScene:(FLTScene *)s        { _engine->destroy((Scene *)[s nativeScene]); }
- (void)destroyView:(FLTView *)v          { _engine->destroy((View *)[v nativeView]); }
- (void)destroyCamera:(FLTCamera *)c      { _engine->destroy((Camera *)[c nativeCamera]); }
- (void)destroySwapChain:(FLTSwapChain *)sc { _engine->destroy((SwapChain *)[sc nativeSwapChain]); }

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
