// ============================================================================
// Meridian
// File: MeridianMemoryService.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// COM-creatable implementation of IMeridianMemoryService (ProgId
// "Meridian.MemoryService").
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

using System.Runtime.InteropServices;
using Meridian.Agents.Memory;
using Meridian.Interop.Com.Configuration;
using Meridian.Interop.Com.Interfaces;
using Meridian.Interop.Com.Support;
using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Interop.Com.Classes;


/// <summary>
/// Class MeridianMemoryService. This class cannot be inherited.
/// Implements the <see cref="IMeridianMemoryService" />
/// </summary>
/// <seealso cref="IMeridianMemoryService" />
[ComVisible(true)]
[Guid("2F0758B8-E17A-4653-B38E-D0B85D99C65F")]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(IMeridianMemoryService))]
[ProgId("Meridian.MemoryService")]
public sealed class MeridianMemoryService : IMeridianMemoryService {
    /// <summary>
    /// The memory
    /// </summary>
    private readonly IMeridianMemory _memory;

    /// <summary>
    /// COM clients create this type via <c>CreateObject("Meridian.MemoryService")</c>, which requires a public parameterless constructor.
    /// </summary>
    public MeridianMemoryService() {
        _memory = MeridianComHost.Services.GetRequiredService<IMeridianMemory>();
    }


    /// <summary>
    /// Saves <paramref name="text" /> to long-term memory.
    /// </summary>
    /// <param name="text">The fact to remember.</param>
    /// <returns>The generated id of the stored memory.</returns>
    public string Remember(string text) => ComSync.Run(() => _memory.RememberAsync(text));


    /// <summary>
    /// Recalls the <paramref name="topK" /> facts most relevant to <paramref name="query" />, as a SAFEARRAY of strings.
    /// </summary>
    /// <param name="query">The natural-language recall query.</param>
    /// <param name="topK">The maximum number of memories to return.</param>
    /// <returns>The matching memory texts, highest relevance first.</returns>
    public string[] Recall(string query, int topK) => ComSync.Run(async () => {
        var entries = await _memory.RecallAsync(query, topK <= 0 ? 5 : topK).ConfigureAwait(false);
        return entries.Select(e => e.Text).ToArray();
    });

    /// <summary>
    /// Forgets a previously remembered fact by id.
    /// </summary>
    /// <param name="id">The id of the memory to forget.</param>
    public void Forget(string id) => ComSync.Run(() => _memory.ForgetAsync(id));
}