// ============================================================================
// Meridian
// File: ComSync.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Bridges Meridian's async APIs to the synchronous calling convention COM
// clients (VB6, Visual FoxPro, Delphi, VBA) require. Runs the async work on a
// thread-pool thread via Task.Run before blocking, so a caller on an STA thread
// with a message pump (the normal case for these hosts) cannot deadlock waiting
// on a continuation that needed that same pump to resume.
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

namespace Meridian.Interop.Com.Support;

/// <summary>
/// Runs async Meridian calls to completion synchronously, safe to call from a COM client's STA thread.
/// </summary>
internal static class ComSync {
    /// <summary>
    /// Runs <paramref name="operation" /> to completion on a thread-pool thread and returns its result.
    /// </summary>
    /// <typeparam name="T">The type of the operation's result.</typeparam>
    /// <param name="operation">The asynchronous operation to run.</param>
    /// <returns>The operation's result.</returns>
    public static T Run<T>(Func<Task<T>> operation) => Task.Run(operation).GetAwaiter().GetResult();

    /// <summary>
    /// Runs <paramref name="operation" /> to completion on a thread-pool thread.
    /// </summary>
    /// <param name="operation">The asynchronous operation to run.</param>
    public static void Run(Func<Task> operation) => Task.Run(operation).GetAwaiter().GetResult();
}
