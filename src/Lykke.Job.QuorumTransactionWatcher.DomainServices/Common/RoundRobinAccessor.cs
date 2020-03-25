using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Job.QuorumTransactionWatcher.DomainServices.Common
{
    public class RoundRobinAccessor<T>
    {
        private readonly ConcurrentQueue<T> _queue;

        public RoundRobinAccessor(IReadOnlyList<T> values)
        {
            if (!values.Any())
                throw new ArgumentException("Values cannot be empty", nameof(values));

            _queue = new ConcurrentQueue<T>(values);
        }

        public T GetNext()
        {
            while (true)
            {
                var dequeued = _queue.TryDequeue(out var next);

                if (dequeued)
                {
                    _queue.Enqueue(next);

                    return next;
                }
            }
        }
    }
}
