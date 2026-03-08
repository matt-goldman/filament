#import "FLTMaterial.h"
#import "FLTMaterialInstance.h"
#import "FLTEngine+Internal.h"
#include <filament/Material.h>
using namespace filament;

@implementation FLTMaterial {
    Material *_material;
}

+ (instancetype)buildWithEngine:(FLTEngine *)engine data:(NSData *)matData {
    FLTMaterial *w = [[FLTMaterial alloc] init];
    w->_material = Material::Builder()
        .package(matData.bytes, matData.length)
        .build(*[engine nativeEngine]);
    return w;
}

- (FLTMaterialInstance *)createInstance {
    return [[FLTMaterialInstance alloc] initWithNative:_material->createInstance()];
}

- (void *)nativeMaterial { return _material; }

@end
