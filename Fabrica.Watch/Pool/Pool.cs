﻿/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Collections.Concurrent;

namespace Fabrica.Watch.Pool;

public class Pool<TPooled>( Func<TPooled> factory, int maxSize = int.MaxValue ) : IDisposable where TPooled : class
{

    private Func<TPooled> Factory { get; } = factory ?? throw new ArgumentNullException(nameof(factory));

    private int MaxSize { get; } = maxSize;

    private ConcurrentQueue<TPooled> Queue { get; } = new();

    private AutoResetEvent AvailableEvent { get; } = new(false);


    public int Count => Queue.Count;

    public void Clear() => Queue.Clear();


    public void Warm( int count )
    {

        for (var i = 0; i < count; i++)
        {
            Queue.Enqueue(Factory());
        }

    }

    public TPooled Acquire( int waitDuration=int.MaxValue )
    {

        TPooled? item;
        do
        {
            if( !Queue.TryDequeue( out item ) && !AvailableEvent.WaitOne( waitDuration ) )
                item = Factory();
        }
        while (null == item);

        return item;

    }

    public void Return( TPooled item )
    {

        if (item == null) throw new ArgumentNullException(nameof(item));

        if( MaxSize > 0 && Queue.Count < MaxSize )
        {
            Queue.Enqueue( item );
            AvailableEvent.Set();
        }

    }

    public void Dispose()
    {

        AvailableEvent.Dispose();
        Queue.Clear();

    }
}