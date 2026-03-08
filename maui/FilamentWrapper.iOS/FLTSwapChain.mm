#import "FLTSwapChain.h"
#include <filament/SwapChain.h>
using namespace filament;

@implementation FLTSwapChain {
    SwapChain *_swapChain;
}

- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine {
    self = [super init];
    if (self) {
        _swapChain = (SwapChain *)native;
    }
    return self;
}

- (void *)nativeSwapChain { return _swapChain; }

@end
