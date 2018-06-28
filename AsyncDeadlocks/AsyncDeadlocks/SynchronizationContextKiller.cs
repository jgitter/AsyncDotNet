using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncDeadlocks
{
    public static class SynchronizationContextKiller
    {
        public static SynchronizationContextKillerDisposable KillItDead()
        {
            var context = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            return new SynchronizationContextKillerDisposable(context);
        }

        public struct SynchronizationContextKillerDisposable : IDisposable
        {
            private readonly SynchronizationContext _synchronizationContext;

            public SynchronizationContextKillerDisposable(SynchronizationContext synchronizationContext)
            {
                _synchronizationContext = synchronizationContext;
            }

            public void Dispose() => SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
        }
    }
}
