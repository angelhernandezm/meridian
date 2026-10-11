// ============================================================================
// Meridian
// File: ServiceCollectionExtensions.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// The single public entry point: services.AddMeridian(options => { ... }),
// mirroring the Add<Framework>(...) convention of ASP.NET Core / Azure
// Functions startup. Everything else in Meridian is reached through the
// MeridianBuilder passed to the configure callback.
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

using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Hosting;

/// <summary>
/// Class ServiceCollectionExtensions.
/// </summary>
public static class ServiceCollectionExtensions {
    /// <summary>
    /// Adds Meridian to <paramref name="services" />. Use the <see cref="MeridianBuilder" /> passed to
    /// <paramref name="configure" /> to opt into the subsystems you need:
    /// <code>
    /// services.AddMeridian(options =&gt; {
    /// options.AddModel(m =&gt; m.AzureOpenAI.ResourceName = "...");
    /// options.AddEmbeddings();
    /// options.AddSemanticStore();
    /// });
    /// </code>
    /// </summary>
    /// <param name="services">The service collection to add Meridian to.</param>
    /// <param name="configure">The callback used to opt into Meridian subsystems via the passed <see cref="MeridianBuilder" />.</param>
    /// <returns><paramref name="services" />, for chaining.</returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    public static IServiceCollection AddMeridian(this IServiceCollection services, Action<MeridianBuilder> configure) {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new MeridianBuilder(services);
        configure(builder);
        return services;
    }
}