#import "FLTScene.h"
#import "FLTIndirectLight.h"
#import "FLTSkybox.h"
#include <filament/Scene.h>
#include <filament/IndirectLight.h>
#include <filament/Skybox.h>
#include <utils/Entity.h>
using namespace filament;

@implementation FLTScene {
    Scene *_scene;
}

- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine {
    self = [super init];
    if (self) {
        _scene = (Scene *)native;
    }
    return self;
}

- (void *)nativeScene { return _scene; }

- (void)addEntity:(uint32_t)entity {
    _scene->addEntity(utils::Entity::import(entity));
}

- (void)removeEntity:(uint32_t)entity {
    _scene->remove(utils::Entity::import(entity));
}

- (void)setIndirectLight:(FLTIndirectLight *)indirectLight {
    _scene->setIndirectLight((IndirectLight *)[indirectLight nativeIndirectLight]);
}

- (void)setSkybox:(FLTSkybox *)skybox {
    _scene->setSkybox((Skybox *)[skybox nativeSkybox]);
}

@end
