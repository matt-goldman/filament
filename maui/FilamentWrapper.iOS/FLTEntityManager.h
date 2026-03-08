#import <Foundation/Foundation.h>

@interface FLTEntityManager : NSObject
/// Returns a new entity ID (uint32). Entity is NOT an object — it is a raw integer handle.
- (uint32_t)create;
- (void)destroy:(uint32_t)entity;
@end
