#import "FLTRenderer.h"
#import "FLTSwapChain.h"
#import "FLTView.h"
#include <filament/Renderer.h>
#include <filament/SwapChain.h>
#include <filament/View.h>
using namespace filament;

@implementation FLTRenderer {
    Renderer *_renderer;
}

- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine {
    self = [super init];
    if (self) {
        _renderer = (Renderer *)native;
    }
    return self;
}

- (void *)nativeRenderer { return _renderer; }

- (BOOL)beginFrame:(FLTSwapChain *)swapChain {
    return _renderer->beginFrame((SwapChain *)[swapChain nativeSwapChain]);
}

- (void)render:(FLTView *)view {
    _renderer->render((View *)[view nativeView]);
}

- (void)endFrame {
    _renderer->endFrame();
}

@end
