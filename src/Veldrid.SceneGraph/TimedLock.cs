using System;
using System.Threading;

namespace Veldrid.SceneGraph
{
#if DEBUG
    public class TimedLock : IDisposable
#else
    public struct TimedLock : IDisposable
#endif
    {
        public static TimedLock Lock (object o)
        {
            return Lock (o, TimeSpan.FromSeconds (10));
        }
 
        public static TimedLock Lock (object o, TimeSpan timeout)
        {
            TimedLock tl = new TimedLock (o);
            if (!Monitor.TryEnter (o, timeout))
            {
#if DEBUG
                System.GC.SuppressFinalize(tl);
#endif
                throw new LockTimeoutException ();
            }
 
            return tl;
        }
 
        private object target;
        private TimedLock (object o)
        {
            target = o;
        }
 
        public void Dispose()
        {
            Monitor.Exit (target);
 
            // It's a bad error if someone forgets to call Dispose,
            // so in Debug builds, we put a finalizer in to detect
            // the error. If Dispose is called, we suppress the
            // finalizer.
#if DEBUG
            GC.SuppressFinalize(this);
#endif
        }
 
#if DEBUG
        ~TimedLock()
        {
            // If this finalizer runs, someone somewhere failed to
            // call Dispose, which means we've failed to leave
            // a monitor!
            System.Diagnostics.Debug.Fail("Undisposed lock");
        }
#endif
    }
 
    public class LockTimeoutException : Exception
    {
        public LockTimeoutException () : base("Timeout waiting for lock")
        {
        }
    }
}