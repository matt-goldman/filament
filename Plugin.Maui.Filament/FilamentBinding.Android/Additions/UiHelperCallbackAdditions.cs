namespace Com.Google.Android.Filament.Android;

/// <summary>
/// Delegate-based adapter for <see cref="UiHelper.IRendererCallback"/>.
/// Allows callers to handle surface lifecycle events via Action delegates
/// instead of creating a custom class that implements the interface.
/// </summary>
public sealed class FilamentRendererCallback : Java.Lang.Object, UiHelper.IRendererCallback
{
    /// <summary>
    /// Invoked when the native window surface becomes available.
    /// The <see cref="global::Android.Views.Surface"/> parameter is the new surface.
    /// Create the Filament SwapChain inside this handler.
    /// </summary>
    public Action<global::Android.Views.Surface?>? NativeWindowChanged { get; set; }

    /// <summary>
    /// Invoked when the native window surface is destroyed.
    /// Destroy the SwapChain and call FlushAndWait() inside this handler.
    /// </summary>
    public Action? DetachedFromSurface { get; set; }

    /// <summary>
    /// Invoked when the surface is resized. Call
    /// <c>FilamentHelper.SynchronizePendingFrames(engine)</c> before updating the viewport.
    /// </summary>
    public Action<int, int>? Resized { get; set; }

    /// <inheritdoc />
    public void OnNativeWindowChanged(global::Android.Views.Surface? p0) =>
        NativeWindowChanged?.Invoke(p0);

    /// <inheritdoc />
    public void OnDetachedFromSurface() =>
        DetachedFromSurface?.Invoke();

    /// <inheritdoc />
    public void OnResized(int width, int height) =>
        Resized?.Invoke(width, height);
}
