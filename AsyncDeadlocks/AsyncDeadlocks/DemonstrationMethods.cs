using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace AsyncDeadlocks
{
    internal class DemonstrationMethods
    {
        /*
         * This method will lock every time unless run in a separate thread.  In a nutshell, the deadlock is caused by contention over
         * the UI thread's context. The call to .Result locks the context, but the attempt at continuation on the async/await needs
         * access to the context to complete execution.
         */
        internal static int FullySynchronousMethod()
        {
            return DoWork().Result;
        }

        /*
         * This method prevents the deadlock by moving the call into a ThreadPool thread which does not have a synchronization context to
         * be deadlocked.  This may seem like the perfect trivial answer for UI and ASP.NET, but threads are expensive.  Use with care.
         */
        internal static async Task<int> WrapSynchronousInAsynchronousMethod()
        {
            return await Task.Run(() => FullySynchronousMethod());
        }

        /*
         * This is the purist "async all the way down" method of preventing deadlocks.
         */
        internal static async Task<int> AsyncAllTheWayDownMethod()
        {
            return await DoWork();
        }

        /*
         * In this case, we prevent deadlock by preventing the continuation of DoWork from glomming onto the UI context.  Instead
         * the DoWork method is, behind the scenes, executed on its own thread / context in parallel.
         */
        internal static int SynchronousWithNoCapturedContinuationMethod()
        {
            return DoWork(false).Result;
        }

        /*
         * The Nito.AsyncEx AsyncContext object allows us to run arbitrary async methods without fear of deadlock.  This method uses
         * Nested Message Loops and will only work if the async method runs single-threaded.  It uses a form of "message pumping"
         * between blocked threads which is a black art not intended for use by mortals and causes re-entrancy.
         *
         * Reentrancy: "In computing, a computer program or subroutine is called reentrant if it can be interrupted in the middle of
         *              its execution and then safely be called again ("re-entered") before its previous invocations complete execution."
         *
         * Be ye wary, for here thar be dragons, yarrr
         */
        internal static int SynchronousUsingStephenClearyMethod()
        {
            return AsyncContext.Run(async () => await DoWork());
        }


        /*
         * This method prevents deadlock by removing the SynchronizationContext.Current entirely, and then replacing it
         * when the Disposable (inner class) is disposed of at the end of the using block.  Nifty workaround... obvious side-effects?
         */
        internal static int SynchronousTemporaryContextRemovalMethod()
        {
            using (SynchronizationContextKiller.KillItDead())
            {
                return DoWork().Result;
            }
        }

        // This is just the asynchronous test method
        internal static async Task<int> DoWork(bool continueOnCapturedContext = true)
        {
            await Task.Delay(100).ConfigureAwait(continueOnCapturedContext);
            return 1;
        }
    }
}
