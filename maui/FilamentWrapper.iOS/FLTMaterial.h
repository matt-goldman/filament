#import <Foundation/Foundation.h>
@class FLTMaterialInstance, FLTEngine;

@interface FLTMaterial : NSObject
+ (instancetype)buildWithEngine:(FLTEngine *)engine data:(NSData *)matData;
- (FLTMaterialInstance *)createInstance;
- (void *)nativeMaterial;
@end
