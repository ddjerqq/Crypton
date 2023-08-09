// <copyright file="AsyncQueue.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Threading.Tasks.Dataflow;

namespace Crypton.Domain.Common;

/// <summary>
///     A thread-safe asynchronous queue.
/// </summary>
/// <example>
///     <code>
/// var queue = new AsyncQueue`int();
/// await foreach(int i in queue) {
///     // Writes a line as soon as some other Task calls queue.Enqueue(..)
///     Console.WriteLine(i);
/// }
/// </code>
/// </example>
/// <remarks>
///     Inspired by an amazing answer on StackOverflow [here](https://stackoverflow.com/a/55912725/14860947).
/// </remarks>
/// <typeparam name="T">
///     The type of the elements in the queue.
/// </typeparam>
public class AsyncQueue<T> : IAsyncEnumerable<T>
{
    private readonly BufferBlock<T> bufferBlock = new();
    private readonly SemaphoreSlim enumerationSemaphore = new(1);

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default)
    {
        try
        {
            await this.enumerationSemaphore.WaitAsync(ct);

            while (!ct.IsCancellationRequested)
                yield return await this.bufferBlock.ReceiveAsync(ct);
        }
        finally
        {
            this.enumerationSemaphore.Release();
        }
    }

    /// <summary>
    ///     Add an item to the queue atomically.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Enqueue(T item)
    {
        this.bufferBlock.Post(item);
    }
}