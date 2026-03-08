using CoreAnimation;
using CoreGraphics;
using Metal;
using UIKit;

namespace Filament.Maui;

/// <summary>
/// UIView subclass that hosts a <see cref="CAMetalLayer"/> for Filament rendering.
/// Overrides <see cref="LayoutSubviews"/> to keep the Metal layer in sync with the
/// view bounds and notify the handler of viewport size changes.
/// </summary>
internal sealed class FilamentMetalView : UIView
{
    /// <summary>The Metal layer used as the Filament swap chain surface.</summary>
    public CAMetalLayer MetalLayer { get; }

    /// <summary>
    /// Callback invoked from <see cref="LayoutSubviews"/> with the new physical pixel
    /// dimensions (width, height) whenever the view is resized.
    /// </summary>
    public Action<int, int>? ViewportResized { get; set; }

    public FilamentMetalView()
    {
        MetalLayer = new CAMetalLayer
        {
            Device = MTLDevice.SystemDefault,
            // CRITICAL: Filament Metal backend requires BGRA8Unorm pixel format.
            // Any other format will cause a crash or incorrect rendering.
            PixelFormat = MTLPixelFormat.BGRA8Unorm,
            Opaque = true,
            ContentsScale = UIScreen.MainScreen.NativeScale,
        };
        Layer.AddSublayer(MetalLayer);
        ContentScaleFactor = UIScreen.MainScreen.NativeScale;
    }

    /// <inheritdoc />
    public override void LayoutSubviews()
    {
        base.LayoutSubviews();
        // Keep the Metal layer frame in sync with the view bounds on every layout pass.
        MetalLayer.Frame = Bounds;
        var w = (int)(Bounds.Width * ContentScaleFactor);
        var h = (int)(Bounds.Height * ContentScaleFactor);
        if (w > 0 && h > 0)
        {
            // DrawableSize must match the physical pixel dimensions so Filament's Metal
            // backend allocates correctly-sized drawables. Without this, layer.drawableSize
            // stays at its default (the logical bounds) and produces blurry output on
            // Retina displays or incorrect resolution after orientation changes.
            MetalLayer.DrawableSize = new CGSize(w, h);
            ViewportResized?.Invoke(w, h);
        }
    }
}
