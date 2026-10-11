// ============================================================================
// Meridian
// File: ModelClients.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// Bundles the Microsoft.Extensions.AI chat and embedding clients produced for a
// single configured provider, plus the human-readable provider label used in
// telemetry and responses.
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

using Microsoft.Extensions.AI;

namespace Meridian.Models.Providers;

/// <summary>
/// The Microsoft.Extensions.AI clients built for one <see cref="ModelProvider" />.
/// </summary>
/// <param name="ProviderName">Human-readable provider label, used in telemetry and responses.</param>
/// <param name="Chat">The chat client. Always present.</param>
/// <param name="Embeddings">The embedding generator, or <c>null</c> when the provider/config has none configured.</param>
/// <param name="Images">The image generator, or <c>null</c> when the provider doesn't support image generation (only AzureOpenAI and OpenAI currently do) or none is configured.</param>
public sealed record ModelClients(string ProviderName, IChatClient Chat, IEmbeddingGenerator<string, Embedding<float>>? Embeddings, IImageGenerator? Images = null);
