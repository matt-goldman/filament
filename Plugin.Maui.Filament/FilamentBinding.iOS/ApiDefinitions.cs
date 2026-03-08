using System;
using Foundation;
using ObjCRuntime;

namespace FilamentBinding.iOS
{
    // =========================================================================
    // FLTEngine
    // Central Filament engine. Use +createWithBackend: to create; call -destroy
    // when done (do not rely on GC / Dispose ordering).
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTEngine
    {
        /// <summary>Creates the Filament engine. Use <see cref="FLTBackend.Metal"/> on iOS.</summary>
        [Static]
        [Export("createWithBackend:")]
        FLTEngine CreateWithBackend(FLTBackend backend);

        /// <summary>Destroys the engine and releases all native resources.</summary>
        [Export("destroy")]
        void Destroy();

        [Export("createRenderer")]
        FLTRenderer CreateRenderer();

        [Export("createScene")]
        FLTScene CreateScene();

        [Export("createView")]
        FLTView CreateView();

        [Export("createCamera")]
        FLTCamera CreateCamera();

        /// <summary>
        /// Creates a swap chain from a <c>CAMetalLayer</c> pointer.
        /// The layer must be configured (<c>pixelFormat = MTLPixelFormatBGRA8Unorm</c>)
        /// before calling this method.
        /// </summary>
        [Export("createSwapChainFromLayer:")]
        FLTSwapChain CreateSwapChainFromLayer(IntPtr nativeLayer);

        [Export("destroyRenderer:")]
        void DestroyRenderer(FLTRenderer renderer);

        [Export("destroyScene:")]
        void DestroyScene(FLTScene scene);

        [Export("destroyView:")]
        void DestroyView(FLTView view);

        /// <summary>Destroys the camera component and its associated entity.</summary>
        [Export("destroyCamera:")]
        void DestroyCamera(FLTCamera camera);

        [Export("destroySwapChain:")]
        void DestroySwapChain(FLTSwapChain swapChain);

        [Export("destroyTexture:")]
        void DestroyTexture(FLTTexture texture);

        [Export("destroyVertexBuffer:")]
        void DestroyVertexBuffer(FLTVertexBuffer vertexBuffer);

        [Export("destroyIndexBuffer:")]
        void DestroyIndexBuffer(FLTIndexBuffer indexBuffer);

        [Export("destroyMaterial:")]
        void DestroyMaterial(FLTMaterial material);

        [Export("destroyMaterialInstance:")]
        void DestroyMaterialInstance(FLTMaterialInstance materialInstance);

        [Export("destroyRenderTarget:")]
        void DestroyRenderTarget(FLTRenderTarget renderTarget);

        [Export("destroyIndirectLight:")]
        void DestroyIndirectLight(FLTIndirectLight indirectLight);

        [Export("destroySkybox:")]
        void DestroySkybox(FLTSkybox skybox);

        /// <summary>Blocks until all pending GPU work has been flushed.</summary>
        [Export("flushAndWait")]
        void FlushAndWait();

        [Export("transformManager")]
        FLTTransformManager TransformManager { get; }

        [Export("renderableManager")]
        FLTRenderableManager RenderableManager { get; }

        [Export("lightManager")]
        FLTLightManager LightManager { get; }

        [Export("entityManager")]
        FLTEntityManager EntityManager { get; }
    }

    // =========================================================================
    // FLTRenderer
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTRenderer
    {
        /// <summary>Begins a new frame; returns <c>true</c> if rendering should proceed.</summary>
        [Export("beginFrame:")]
        bool BeginFrame(FLTSwapChain swapChain);

        [Export("render:")]
        void Render(FLTView view);

        [Export("endFrame")]
        void EndFrame();

        /// <summary>Sets the clear color applied at the start of each frame.</summary>
        [Export("setClearColorRed:green:blue:alpha:")]
        void SetClearColor(float r, float g, float b, float a);
    }

    // =========================================================================
    // FLTView
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTView
    {
        [Export("setScene:")]
        void SetScene(FLTScene scene);

        [Export("setCamera:")]
        void SetCamera(FLTCamera camera);

        [Export("setViewportLeft:bottom:width:height:")]
        void SetViewport(int left, int bottom, uint width, uint height);

        [Export("setPostProcessingEnabled:")]
        void SetPostProcessingEnabled(bool enabled);
    }

    // =========================================================================
    // FLTScene
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTScene
    {
        /// <summary>Adds an entity (by its raw uint32 ID) to the scene.</summary>
        [Export("addEntity:")]
        void AddEntity(uint entity);

        /// <summary>Removes an entity (by its raw uint32 ID) from the scene.</summary>
        [Export("removeEntity:")]
        void RemoveEntity(uint entity);

        [Export("setIndirectLight:")]
        void SetIndirectLight(FLTIndirectLight indirectLight);

        [Export("setSkybox:")]
        void SetSkybox(FLTSkybox skybox);
    }

    // =========================================================================
    // FLTCamera
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTCamera
    {
        /// <summary>The raw entity ID (uint32) backing this camera component.</summary>
        [Export("entityId")]
        uint EntityId { get; }

        [Export("setProjectionFov:aspect:near:far:")]
        void SetProjection(double fovDegrees, double aspect, double near, double far);

        [Export("lookAtEyeX:eyeY:eyeZ:centerX:centerY:centerZ:upX:upY:upZ:")]
        void LookAt(double eyeX, double eyeY, double eyeZ,
                    double centerX, double centerY, double centerZ,
                    double upX, double upY, double upZ);
    }

    // =========================================================================
    // FLTSwapChain
    // Created by FLTEngine.createSwapChainFromLayer: — not instantiated directly.
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTSwapChain
    {
    }

    // =========================================================================
    // FLTMaterial
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTMaterial
    {
        /// <summary>Builds a material from compiled material data (a <c>.mat</c> file blob).</summary>
        [Static]
        [Export("buildWithEngine:data:")]
        FLTMaterial BuildWithEngine(FLTEngine engine, NSData matData);

        [Export("createInstance")]
        FLTMaterialInstance CreateInstance();
    }

    // =========================================================================
    // FLTMaterialInstance
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTMaterialInstance
    {
        [Export("setFloatParameter:value:")]
        void SetFloatParameter(string name, float value);

        [Export("setFloat4Parameter:value:")]
        void SetFloat4Parameter(string name, VectorFloat4 value);

        [Export("setTextureParameter:texture:")]
        void SetTextureParameter(string name, FLTTexture texture);
    }

    // =========================================================================
    // FLTTextureBuilder
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTTextureBuilder
    {
        [Export("width:")]
        FLTTextureBuilder Width(uint width);

        [Export("height:")]
        FLTTextureBuilder Height(uint height);

        [Export("depth:")]
        FLTTextureBuilder Depth(uint depth);

        [Export("levels:")]
        FLTTextureBuilder Levels(byte levels);

        [Export("sampler:")]
        FLTTextureBuilder Sampler(FLTTextureSamplerType samplerType);

        [Export("format:")]
        FLTTextureBuilder Format(FLTTextureFormat format);

        [Export("usage:")]
        FLTTextureBuilder Usage(FLTTextureUsage usage);

        [Export("buildWithEngine:")]
        FLTTexture BuildWithEngine(FLTEngine engine);
    }

    // =========================================================================
    // FLTTexture
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTTexture
    {
        [Static]
        [Export("builder")]
        FLTTextureBuilder Builder();

        /// <summary>
        /// Uploads pixel data to the specified mip level.
        /// The contents of <paramref name="data"/> are copied immediately.
        /// </summary>
        [Export("setImage:level:data:")]
        void SetImage(FLTEngine engine, nuint level, NSData data);
    }

    // =========================================================================
    // FLTRenderTargetBuilder
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTRenderTargetBuilder
    {
        [Export("texture:attachment:")]
        FLTRenderTargetBuilder SetTexture(FLTTexture texture, FLTAttachmentPoint attachment);

        [Export("mipLevel:attachment:")]
        FLTRenderTargetBuilder MipLevel(byte level, FLTAttachmentPoint attachment);

        [Export("buildWithEngine:")]
        FLTRenderTarget BuildWithEngine(FLTEngine engine);
    }

    // =========================================================================
    // FLTRenderTarget
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTRenderTarget
    {
        [Static]
        [Export("builder")]
        FLTRenderTargetBuilder Builder();
    }

    // =========================================================================
    // FLTEntityManager
    // Entity IDs are raw uint32 values, not objects.
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTEntityManager
    {
        /// <summary>Creates a new entity and returns its raw uint32 ID.</summary>
        [Export("create")]
        uint Create();

        /// <summary>Destroys the entity with the given raw uint32 ID.</summary>
        [Export("destroy:")]
        void Destroy(uint entity);
    }

    // =========================================================================
    // FLTTransformManager
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTTransformManager
    {
        [Export("createComponent:")]
        void CreateComponent(uint entity);

        [Export("destroyComponent:")]
        void DestroyComponent(uint entity);

        /// <summary>
        /// Sets the local transform of <paramref name="entity"/> to <paramref name="transform"/>.
        /// <paramref name="transform"/> is a column-major 4×4 float matrix
        /// (matches <c>simd_float4x4</c> memory layout).
        /// </summary>
        [Export("setTransform:forEntity:")]
        void SetTransform(MatrixFloat4x4 transform, uint entity);

        [Export("getTransformForEntity:")]
        MatrixFloat4x4 GetTransform(uint entity);
    }

    // =========================================================================
    // FLTRenderableManagerBuilder
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTRenderableManagerBuilder
    {
        [Export("geometryAtIndex:primitiveType:vertexBuffer:indexBuffer:")]
        FLTRenderableManagerBuilder GeometryAtIndex(nint index, FLTPrimitiveType type,
                                                    FLTVertexBuffer vertexBuffer,
                                                    FLTIndexBuffer indexBuffer);

        [Export("materialAtIndex:materialInstance:")]
        FLTRenderableManagerBuilder MaterialAtIndex(nint index, FLTMaterialInstance materialInstance);

        [Export("boundingBoxCenterX:centerY:centerZ:halfExtentX:halfExtentY:halfExtentZ:")]
        FLTRenderableManagerBuilder BoundingBox(float cx, float cy, float cz,
                                                float halfExtentX, float halfExtentY, float halfExtentZ);

        [Export("castShadows:")]
        FLTRenderableManagerBuilder CastShadows(bool enable);

        [Export("receiveShadows:")]
        FLTRenderableManagerBuilder ReceiveShadows(bool enable);

        [Export("buildWithEngine:entity:")]
        void BuildWithEngine(FLTEngine engine, uint entity);
    }

    // =========================================================================
    // FLTRenderableManager
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTRenderableManager
    {
        [Export("builderWithCount:")]
        FLTRenderableManagerBuilder BuilderWithCount(nint count);

        [Export("destroyComponent:")]
        void DestroyComponent(uint entity);
    }

    // =========================================================================
    // FLTLightManagerBuilder
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTLightManagerBuilder
    {
        [Export("color:")]
        FLTLightManagerBuilder Color(VectorFloat3 color);

        [Export("intensity:")]
        FLTLightManagerBuilder Intensity(float intensity);

        [Export("direction:")]
        FLTLightManagerBuilder Direction(VectorFloat3 direction);

        [Export("castShadows:")]
        FLTLightManagerBuilder CastShadows(bool enable);

        [Export("position:")]
        FLTLightManagerBuilder Position(VectorFloat3 position);

        [Export("buildWithEngine:entity:")]
        void BuildWithEngine(FLTEngine engine, uint entity);
    }

    // =========================================================================
    // FLTLightManager
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTLightManager
    {
        [Export("builderWithType:")]
        FLTLightManagerBuilder BuilderWithType(FLTLightType type);

        [Export("destroyComponent:")]
        void DestroyComponent(uint entity);
    }

    // =========================================================================
    // FLTIndirectLightBuilder
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTIndirectLightBuilder
    {
        [Export("reflections:")]
        FLTIndirectLightBuilder Reflections(FLTTexture cubemap);

        /// <summary>
        /// Sets irradiance as spherical harmonics.
        /// <paramref name="bands"/> must be 1, 2, or 3.
        /// <paramref name="shData"/> must contain <c>bands²</c> × <see cref="VectorFloat3"/> coefficients.
        /// </summary>
        [Export("irradianceBands:data:")]
        FLTIndirectLightBuilder IrradianceBands(byte bands, NSData shData);

        [Export("intensity:")]
        FLTIndirectLightBuilder Intensity(float envIntensity);

        [Export("buildWithEngine:")]
        FLTIndirectLight BuildWithEngine(FLTEngine engine);
    }

    // =========================================================================
    // FLTIndirectLight
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTIndirectLight
    {
        [Static]
        [Export("builder")]
        FLTIndirectLightBuilder Builder();
    }

    // =========================================================================
    // FLTSkyboxBuilder
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTSkyboxBuilder
    {
        [Export("environment:")]
        FLTSkyboxBuilder Environment(FLTTexture cubemap);

        [Export("showSun:")]
        FLTSkyboxBuilder ShowSun(bool show);

        [Export("color:")]
        FLTSkyboxBuilder Color(VectorFloat4 color);

        [Export("buildWithEngine:")]
        FLTSkybox BuildWithEngine(FLTEngine engine);
    }

    // =========================================================================
    // FLTSkybox
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTSkybox
    {
        [Static]
        [Export("builder")]
        FLTSkyboxBuilder Builder();
    }

    // =========================================================================
    // FLTVertexBufferBuilder
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTVertexBufferBuilder
    {
        [Export("vertexCount:")]
        FLTVertexBufferBuilder VertexCount(uint count);

        [Export("bufferCount:")]
        FLTVertexBufferBuilder BufferCount(byte count);

        [Export("attribute:bufferIndex:attributeType:byteOffset:byteStride:")]
        FLTVertexBufferBuilder Attribute(FLTVertexAttribute attribute, byte bufferIndex,
                                         FLTVertexAttributeType attributeType,
                                         uint byteOffset, byte byteStride);

        [Export("normalizedAttribute:normalized:")]
        FLTVertexBufferBuilder NormalizedAttribute(FLTVertexAttribute attribute, bool normalized);

        [Export("buildWithEngine:")]
        FLTVertexBuffer BuildWithEngine(FLTEngine engine);
    }

    // =========================================================================
    // FLTVertexBuffer
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTVertexBuffer
    {
        [Static]
        [Export("builder")]
        FLTVertexBufferBuilder Builder();

        /// <summary>
        /// Uploads vertex data for the given buffer slot.
        /// The contents of <paramref name="data"/> are copied immediately.
        /// </summary>
        [Export("setBufferAtIndex:engine:data:")]
        void SetBufferAtIndex(byte bufferIndex, FLTEngine engine, NSData data);
    }

    // =========================================================================
    // FLTIndexBufferBuilder
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTIndexBufferBuilder
    {
        [Export("indexCount:")]
        FLTIndexBufferBuilder IndexCount(uint count);

        [Export("bufferType:")]
        FLTIndexBufferBuilder BufferType(FLTIndexType type);

        [Export("buildWithEngine:")]
        FLTIndexBuffer BuildWithEngine(FLTEngine engine);
    }

    // =========================================================================
    // FLTIndexBuffer
    // =========================================================================
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface FLTIndexBuffer
    {
        [Static]
        [Export("builder")]
        FLTIndexBufferBuilder Builder();

        /// <summary>
        /// Uploads index data.
        /// The contents of <paramref name="data"/> are copied immediately.
        /// </summary>
        [Export("setBufferWithEngine:data:")]
        void SetBuffer(FLTEngine engine, NSData data);
    }
}
