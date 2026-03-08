#import "FLTView.h"
#import "FLTScene.h"
#import "FLTCamera.h"
#include <filament/View.h>
#include <filament/Viewport.h>
using namespace filament;

@implementation FLTView {
    View *_view;
}

- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine {
    self = [super init];
    if (self) {
        _view = (View *)native;
    }
    return self;
}

- (void *)nativeView { return _view; }

- (void)setScene:(FLTScene *)scene {
    _view->setScene((Scene *)[scene nativeScene]);
}

- (void)setCamera:(FLTCamera *)camera {
    _view->setCamera((Camera *)[camera nativeCamera]);
}

- (void)setViewportLeft:(int)left bottom:(int)bottom width:(uint32_t)width height:(uint32_t)height {
    _view->setViewport({left, bottom, width, height});
}

- (void)setClearColorRed:(float)r green:(float)g blue:(float)b alpha:(float)a {
    _view->setBackgroundColor({r, g, b, a});
}

- (void)setPostProcessingEnabled:(BOOL)enabled {
    _view->setPostProcessingEnabled(enabled);
}

@end
