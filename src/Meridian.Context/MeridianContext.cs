// ============================================================================
// Meridian
// File: MeridianContext.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// The ambient execution context flowed through a single logical operation (a
// chat turn, an agent run, a retrieval call). Carries correlation/tenant/user
// identifiers and an open-ended property bag that Models, Agents, Knowledge and
// Observability can all read without taking a dependency on one another.
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

using System.Collections.Concurrent;

namespace Meridian.Context;

/// <summary>
/// Immutable-by-convention ambient context for a single Meridian operation.
/// </summary>
public sealed class MeridianContext {
    /// <summary>
    /// Gets the conversation this operation belongs to.
    /// </summary>
    /// <value>A newly generated <see cref="ConversationId"/> unless overridden at initialization.</value>
    public ConversationId ConversationId { get; init; } = ConversationId.New();

    /// <summary>
    /// Gets the correlation id used to stitch together logs/traces/metrics for this operation.
    /// </summary>
    /// <value>A freshly generated GUID string unless overridden at initialization.</value>
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString("n");

    /// <summary>
    /// Gets the calling user id, when known.
    /// </summary>
    /// <value>The user id, or <see langword="null"/> when unknown.</value>
    public string? UserId { get; init; }

    /// <summary>
    /// Gets the tenant id, when Meridian is hosted multi-tenant.
    /// </summary>
    /// <value>The tenant id, or <see langword="null"/> when not multi-tenant.</value>
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets an open-ended bag of properties for cross-cutting concerns that don't warrant a first-class field.
    /// </summary>
    /// <value>A thread-safe dictionary, empty by default.</value>
    public ConcurrentDictionary<string, object?> Properties { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Creates a new context, optionally copying identity fields from <paramref name="parent"/>.
    /// </summary>
    /// <param name="parent">The context to copy <see cref="UserId"/>, <see cref="TenantId"/> and <see cref="ConversationId"/> from, or <see langword="null"/> to start fresh.</param>
    /// <returns>A new <see cref="MeridianContext"/>.</returns>
    public static MeridianContext Create(MeridianContext? parent = null) => new() {
        UserId = parent?.UserId,
        TenantId = parent?.TenantId,
        ConversationId = parent?.ConversationId ?? ConversationId.New()
    };
}
