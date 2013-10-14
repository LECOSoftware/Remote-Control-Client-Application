// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.Threading;

namespace CornerstoneRemoteControlClient.Helpers
{
    //Taken From: http://www.interact-sw.co.uk/iangblog/2004/03/23/locking

    // Thanks to Eric Gunnerson for recommending this be a struct rather
    // than a class - avoids a heap allocation.
    // (In Debug mode, we make it a class so that we can add a finalizer
    // in order to detect when the object is not freed.)
    // Thanks to Chance Gillespie and Jocelyn Coulmance for pointing out
    // the bugs that then crept in when I changed it to use struct...

    public struct TimedLock : IDisposable
    {
        private object _target;
        private int _threadId;

        public TimedLock(object o)
            : this(o, TimeSpan.FromSeconds(10))
        {
        }

        public TimedLock(object o, TimeSpan timeout)
        {
            _target = o;
            _threadId = Thread.CurrentThread.ManagedThreadId;

            if (!Monitor.TryEnter(o, timeout))
            {
                throw new LockTimeoutException();
            }
        }

        public void Dispose()
        {
            Monitor.Exit(_target);
            _threadId = 0;
            _target = null;
        }

        public void VerifyLocked()
        {
            if (_threadId != Thread.CurrentThread.ManagedThreadId)
            {
                throw new UnlockedException();
            }
        }

        public void VerifyLocked(String msg)
        {
            if (_threadId != Thread.CurrentThread.ManagedThreadId)
            {
                throw new UnlockedException(msg);
            }
        }

        public Boolean CheckLocked()
        {
            return (_threadId == Thread.CurrentThread.ManagedThreadId);
        }
    }

    public class LockTimeoutException : Exception
    {
        public LockTimeoutException()
            : base("Timeout waiting for lock")
        {
        }
    }

    public class UnlockedException : Exception
    {
        public UnlockedException(String msg)
            : base(msg)
        {
        }

        public UnlockedException()
            : base("Accessed without first locking")
        {
        }
    }
}
