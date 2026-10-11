// ============================================================================
// Meridian
// File: MeridianChatService.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// COM-creatable implementation of IMeridianChatService (ProgId
// "Meridian.ChatService"). Resolves the shared IChatClient from
// MeridianComHost on construction and bridges each call to a synchronous,
// COM-friendly method via ComSync.
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
using Meridian.Interop.Com.Configuration;
using Meridian.Interop.Com.Interfaces;
using Meridian.Interop.Com.Support;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Interop.Com.Classes;

/// <summary>
/// Class MeridianChatService. This class cannot be inherited.
/// Implements the <see cref="IMeridianChatService" />
/// </summary>
/// <seealso cref="IMeridianChatService" />
[ComVisible(true)]
[Guid("C9F8EF7E-3E31-477F-86AE-87C87E132CD6")]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(IMeridianChatService))]
[ProgId("Meridian.ChatService")]
public sealed class MeridianChatService : IMeridianChatService {
    /// <summary>
    /// The chat client
    /// </summary>
    private readonly IChatClient _chatClient;

    /// <summary>
    /// COM clients create this type via <c>CreateObject("Meridian.ChatService")</c>, which requires a public parameterless constructor.
    /// </summary>
    public MeridianChatService() {
        _chatClient = MeridianComHost.Services.GetRequiredService<IChatClient>();
    }

    /// <summary>
    /// Sends <paramref name="prompt" /> to the configured default model and returns its text reply.
    /// </summary>
    /// <param name="prompt">The user prompt.</param>
    /// <returns>The model's text reply.</returns>
    public string Chat(string prompt) => ComSync.Run(async () => {
        var response = await _chatClient.GetResponseAsync([new ChatMessage(ChatRole.User, prompt)]).ConfigureAwait(false);
        return response.Text;
    });


    /// <summary>
    /// Sends <paramref name="prompt" /> with a system instruction and returns the model's text reply.
    /// </summary>
    /// <param name="instructions">The system instructions (persona, rules) to apply to this call.</param>
    /// <param name="prompt">The user prompt.</param>
    /// <returns>The model's text reply.</returns>
    public string ChatWithInstructions(string instructions, string prompt) => ComSync.Run(async () => {
        List<ChatMessage> messages = [new(ChatRole.System, instructions), new(ChatRole.User, prompt)];
        var response = await _chatClient.GetResponseAsync(messages).ConfigureAwait(false);
        return response.Text;
    });
}