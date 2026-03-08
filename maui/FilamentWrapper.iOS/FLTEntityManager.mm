#import "FLTEntityManager.h"
#include <utils/EntityManager.h>
#include <utils/Entity.h>

@implementation FLTEntityManager

- (uint32_t)create {
    utils::Entity e = utils::EntityManager::get().create();
    return e.getId();
}

- (void)destroy:(uint32_t)entity {
    utils::Entity e = utils::Entity::import(entity);
    utils::EntityManager::get().destroy(e);
}

@end
