#import <Foundation/Foundation.h>
@class FLTIndirectLight, FLTSkybox, FLTEngine;

@interface FLTScene : NSObject
- (instancetype)initWithNative:(void *)native engine:(FLTEngine *)engine;
- (void *)nativeScene;
- (void)addEntity:(uint32_t)entity;
- (void)removeEntity:(uint32_t)entity;
- (void)setIndirectLight:(FLTIndirectLight *)indirectLight;
- (void)setSkybox:(FLTSkybox *)skybox;
@end
