#import <Foundation/Foundation.h>
@class FLTRenderer, FLTScene, FLTView, FLTCamera, FLTSwapChain;
@class FLTTransformManager, FLTRenderableManager, FLTLightManager, FLTEntityManager;

typedef NS_ENUM(NSInteger, FLTBackend) {
    FLTBackendDefault  = 0,
    FLTBackendOpenGL   = 1,
    FLTBackendVulkan   = 2,
    FLTBackendMetal    = 3,
};

@interface FLTEngine : NSObject
/// Creates the Filament engine. Use FLTBackendMetal on iOS.
+ (instancetype)createWithBackend:(FLTBackend)backend;
- (void)destroy;
- (FLTRenderer *)createRenderer;
- (FLTScene *)createScene;
- (FLTView *)createView;
- (FLTCamera *)createCamera;
/// nativeLayer must be (__bridge void*)caMetalLayer.
/// CAMetalLayer must be configured (pixelFormat = MTLPixelFormatBGRA8Unorm) before calling.
- (FLTSwapChain *)createSwapChainFromLayer:(void *)nativeLayer;
- (void)destroyRenderer:(FLTRenderer *)renderer;
- (void)destroyScene:(FLTScene *)scene;
- (void)destroyView:(FLTView *)view;
- (void)destroyCamera:(FLTCamera *)camera;
- (void)destroySwapChain:(FLTSwapChain *)swapChain;
- (void)flushAndWait;
- (FLTTransformManager *)transformManager;
- (FLTRenderableManager *)renderableManager;
- (FLTLightManager *)lightManager;
- (FLTEntityManager *)entityManager;
@end
