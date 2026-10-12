// ============================================================================
// Meridian
// File: MeridianComHost.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Process-wide, lazily-built IServiceProvider for the COM surface. A COM client
// creates a fresh ComXxxService instance per CoCreateInstance call, but they all
// share this one Meridian pipeline (model clients, embeddings, memory, agents),
// matching how services.AddMeridian(...) is meant to be configured once.
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

using Meridian.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Interop.Com.Configuration;

/// <summary>
/// Owns the single <see cref="IServiceProvider" /> every COM-visible Meridian class resolves from.
/// </summary>
internal static class MeridianComHost {
    /// <summary>
    /// The s services
    /// </summary>
    private static readonly Lazy<IServiceProvider> s_services = new(Build, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Gets the shared service provider, built on first access.
    /// </summary>
    /// <value>The process-wide Meridian <see cref="IServiceProvider" /> for the COM surface.</value>
    public static IServiceProvider Services => s_services.Value;

    /// <summary>
    /// Builds the Meridian service provider, configured entirely from environment variables.
    /// </summary>
    /// <returns>The built <see cref="IServiceProvider" />.</returns>
    private static IServiceProvider Build() {
        var services = new ServiceCollection();

        services.AddMeridian(options => {
            options.AddModel(ComInteropConfigurator.Apply);
            options.AddEmbeddings();
            options.AddSemanticStore();
            options.AddHybridRetrieval();
            options.AddRagContextBuilder();
            options.AddMemory();
            options.AddImages();

            // The COM surface's one built-in agent, reachable as CreateObject("Meridian.AgentService")
            // .CreateSession("assistant"). A COM caller can't run the C# fluent builder to register its
            // own agents, so this is the one every VB6/VFP/Delphi/VBA host gets by default.
            options.AddAgent("assistant", agent => {
                agent.Instructions = "You are a concise, helpful assistant.";
                agent.EnableSearchTool = true;
            });
        });

        return services.BuildServiceProvider();
    }
}