using System;
using System.Runtime.InteropServices;
using ObjCRuntime;

namespace FilamentBinding.iOS
{
    /// <summary>
    /// Rendering backend. Use <see cref="Metal"/> on iOS.
    /// Maps to the ObjC <c>FLTBackend</c> NS_ENUM (NSInteger).
    /// </summary>
    [Native]
    public enum FLTBackend : long
    {
        Default = 0,
        OpenGL  = 1,
        Vulkan  = 2,
        Metal   = 3,
    }

    /// <summary>
    /// Internal texture format.
    /// Maps to the ObjC <c>FLTTextureFormat</c> NS_ENUM (NSUInteger).
    /// </summary>
    [Native]
    public enum FLTTextureFormat : ulong
    {
        Rgba8    = 0,
        Rgb8     = 1,
        Rgba16F  = 2,
        Rgb16F   = 3,
        R8       = 4,
        Depth32F = 5,
    }

    /// <summary>
    /// Texture usage flags.
    /// Maps to the ObjC <c>FLTTextureUsage</c> NS_OPTIONS (NSUInteger).
    /// </summary>
    [Native, Flags]
    public enum FLTTextureUsage : ulong
    {
        ColorAttachment   = 0x1,
        DepthAttachment   = 0x2,
        StencilAttachment = 0x4,
        Uploadable        = 0x8,
        Sampleable        = 0x10,
        Default           = 0x18,
    }

    /// <summary>
    /// Texture sampler dimensionality.
    /// Maps to the ObjC <c>FLTTextureSamplerType</c> NS_ENUM (NSUInteger).
    /// </summary>
    [Native]
    public enum FLTTextureSamplerType : ulong
    {
        Sampler2D      = 0,
        CubeMap        = 1,
        Sampler2DArray = 2,
    }

    /// <summary>
    /// Render target attachment point.
    /// Maps to the ObjC <c>FLTAttachmentPoint</c> NS_ENUM (NSUInteger).
    /// </summary>
    [Native]
    public enum FLTAttachmentPoint : ulong
    {
        Color0 = 0,
        Color1 = 1,
        Color2 = 2,
        Color3 = 3,
        Depth  = 4,
    }

    /// <summary>
    /// Primitive topology used by <see cref="FLTRenderableManagerBuilder"/>.
    /// Maps to the ObjC <c>FLTPrimitiveType</c> NS_ENUM (NSUInteger).
    /// </summary>
    [Native]
    public enum FLTPrimitiveType : ulong
    {
        Points        = 0,
        Lines         = 1,
        LineStrip     = 2,
        Triangles     = 3,
        TriangleStrip = 4,
    }

    /// <summary>
    /// Light source type used by <see cref="FLTLightManagerBuilder"/>.
    /// Maps to the ObjC <c>FLTLightType</c> NS_ENUM (NSUInteger).
    /// </summary>
    [Native]
    public enum FLTLightType : ulong
    {
        Sun         = 0,
        Directional = 1,
        Point       = 2,
        FocusedSpot = 3,
        Spot        = 4,
    }

    /// <summary>
    /// Vertex attribute semantic slot.
    /// Maps to the ObjC <c>FLTVertexAttribute</c> NS_ENUM (NSUInteger).
    /// </summary>
    [Native]
    public enum FLTVertexAttribute : ulong
    {
        Position    = 0,
        Tangents    = 1,
        Color       = 2,
        Uv0         = 3,
        Uv1         = 4,
        BoneIndices = 5,
        BoneWeights = 6,
        Custom0     = 7,
        Custom1     = 8,
    }

    /// <summary>
    /// Vertex attribute data type.
    /// Maps to the ObjC <c>FLTVertexAttributeType</c> NS_ENUM (NSUInteger).
    /// </summary>
    [Native]
    public enum FLTVertexAttributeType : ulong
    {
        Byte    = 0,
        Byte2   = 1,
        Byte3   = 2,
        Byte4   = 3,
        UByte   = 4,
        UByte2  = 5,
        UByte3  = 6,
        UByte4  = 7,
        Short   = 8,
        Short2  = 9,
        Short3  = 10,
        Short4  = 11,
        UShort  = 12,
        UShort2 = 13,
        UShort3 = 14,
        UShort4 = 15,
        Int     = 16,
        UInt    = 17,
        Float   = 18,
        Float2  = 19,
        Float3  = 20,
        Float4  = 21,
        Half    = 22,
        Half2   = 23,
        Half3   = 24,
        Half4   = 25,
    }

    /// <summary>
    /// Index buffer element width.
    /// Maps to the ObjC <c>FLTIndexType</c> NS_ENUM (NSUInteger).
    /// </summary>
    [Native]
    public enum FLTIndexType : ulong
    {
        UShort = 0,
        UInt   = 1,
    }

    // -------------------------------------------------------------------------
    // Value types that map to Apple SIMD types used in wrapper method signatures.
    // These structs are blittable and match the in-memory layout of the
    // corresponding <simd/simd.h> types on arm64 iOS.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Blittable struct matching <c>simd_float3</c> (padded to 16 bytes for
    /// SIMD alignment, as required by the arm64 ABI).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VectorFloat3
    {
        public float X;
        public float Y;
        public float Z;
        /// <summary>Padding to match the 16-byte alignment of <c>simd_float3</c>.</summary>
        private float _padding;

        public VectorFloat3(float x, float y, float z)
        {
            X = x; Y = y; Z = z; _padding = 0f;
        }
    }

    /// <summary>
    /// Blittable struct matching <c>simd_float4</c> (16 bytes).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VectorFloat4
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public VectorFloat4(float x, float y, float z, float w)
        {
            X = x; Y = y; Z = z; W = w;
        }
    }

    /// <summary>
    /// Blittable struct matching <c>simd_float4x4</c> (64 bytes, column-major).
    /// Each field Cx_Ry is column <c>x</c>, row <c>y</c> to match Filament's
    /// column-major convention.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MatrixFloat4x4
    {
        // Column 0
        public float C0R0, C0R1, C0R2, C0R3;
        // Column 1
        public float C1R0, C1R1, C1R2, C1R3;
        // Column 2
        public float C2R0, C2R1, C2R2, C2R3;
        // Column 3
        public float C3R0, C3R1, C3R2, C3R3;

        /// <summary>Returns the identity matrix.</summary>
        public static MatrixFloat4x4 Identity => new MatrixFloat4x4
        {
            C0R0 = 1f, C1R1 = 1f, C2R2 = 1f, C3R3 = 1f,
        };
    }
}
