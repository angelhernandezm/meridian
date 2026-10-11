// ============================================================================
// Meridian
// File: Program.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Entry point for the cross-platform Avalonia test harness. Boots Avalonia with
// the Fluent theme and Inter font (needed for correct glyph rendering on Linux,
// which has no guaranteed system font), then runs MainWindow on the classic
// desktop lifetime.
//
// License: MIT
// ============================================================================
//
// MIT License
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ============================================================================

using Avalonia;

namespace Meridian.Harness;

/// <summary>
/// The harness application's entry point.
/// </summary>
public static class Program {
    /// <summary>
    /// Process entry point.
    /// </summary>
    /// <param name="args">Command-line arguments, forwarded to Avalonia.</param>
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    /// <summary>
    /// Builds the Avalonia <see cref="AppBuilder" />: platform auto-detection, the Inter font
    /// (needed for reliable glyph rendering on Linux), and logging to the console for diagnostics.
    /// </summary>
    /// <returns>The configured <see cref="AppBuilder" />.</returns>
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace();
}
