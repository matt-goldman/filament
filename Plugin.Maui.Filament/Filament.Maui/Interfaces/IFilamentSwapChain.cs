namespace Filament.Maui;

/// <summary>
/// Opaque handle to a platform-native window surface (Android: <c>Surface</c>;
/// iOS: <c>CAMetalLayer</c>). Created via <see cref="IFilamentEngine.CreateSwapChain"/>.
/// </summary>
public interface IFilamentSwapChain : IDisposable { }
