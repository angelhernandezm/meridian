// ============================================================================
// Meridian
// File: IMeridianChatService.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// COM-visible, dual-interface contract for stateless chat: one prompt in, one
// text reply out. No conversation history is kept across calls — use
// IMeridianAgentService for multi-turn, session-based conversations.
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

namespace Meridian.Interop.Com.Interfaces;

/// <summary>
/// Stateless, single-turn chat, callable from VB6/VFP/Delphi/VBA.
/// </summary>
[ComVisible(true)]
[Guid("2D07154A-839F-4434-B531-F903730453A8")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface IMeridianChatService {
    /// <summary>
    /// Sends <paramref name="prompt" /> to the configured default model and returns its text reply.
    /// </summary>
    /// <param name="prompt">The user prompt.</param>
    /// <returns>The model's text reply.</returns>
    string Chat(string prompt);

    /// <summary>
    /// Sends <paramref name="prompt" /> with a system instruction and returns the model's text reply.
    /// </summary>
    /// <param name="instructions">The system instructions (persona, rules) to apply to this call.</param>
    /// <param name="prompt">The user prompt.</param>
    /// <returns>The model's text reply.</returns>
    string ChatWithInstructions(string instructions, string prompt);
}