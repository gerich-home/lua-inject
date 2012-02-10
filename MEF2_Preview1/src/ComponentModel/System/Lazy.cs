// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
#if !CLR40
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Microsoft.Internal;

namespace System.Threading
{
    public enum LazyThreadSafetyMode
    {
        None,
        PublicationOnly,
        ExecutionAndPublication
    }
}

namespace System
{
    [Serializable]
    public class Lazy<T>
    {

        #region Inner classes
        [Serializable]
        class Boxed
        {
            internal Boxed(T value)
            {
                m_value = value;
            }
            internal T m_value;
        }

        class LazyInternalExceptionHolder
        {
            internal Exception m_exception;
            internal LazyInternalExceptionHolder(Exception ex)
            {
                m_exception = ex;
            }
        }
        #endregion

        static Func<T> PUBLICATION_ONLY_OR_ALREADY_INITIALIZED = delegate { return default(T); };

        private volatile object m_boxed;

        [NonSerialized]
        private Func<T> m_valueFactory;

        [NonSerialized]
        private readonly object m_threadSafeObj;

        public Lazy()
            : this(LazyThreadSafetyMode.ExecutionAndPublication)
        {
        }

        public Lazy(Func<T> valueFactory)
            : this(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication)
        {
        }

        public Lazy(bool isThreadSafe) :
            this(isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
        {
        }

        public Lazy(LazyThreadSafetyMode mode)
        {
            m_threadSafeObj = GetObjectFromMode(mode);
        }

        public Lazy(Func<T> valueFactory, bool isThreadSafe)
            : this(valueFactory, isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
        {
        }

        public Lazy(Func<T> valueFactory, LazyThreadSafetyMode mode)
        {
            if (valueFactory == null)
                throw new ArgumentNullException("valueFactory");

            m_threadSafeObj = GetObjectFromMode(mode);
            m_valueFactory = valueFactory;
        }

        private static object GetObjectFromMode(LazyThreadSafetyMode mode)
        {
            if (mode == LazyThreadSafetyMode.ExecutionAndPublication)
                return new object();
            else if (mode == LazyThreadSafetyMode.PublicationOnly)
                return PUBLICATION_ONLY_OR_ALREADY_INITIALIZED;
            else if (mode != LazyThreadSafetyMode.None)
                throw new ArgumentOutOfRangeException("mode", "Invalid mode");

            return null; // None mode
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            // Force initialization
            T dummy = Value;
        }

        internal LazyThreadSafetyMode Mode
        {
            get
            {
                if (m_threadSafeObj == null) return LazyThreadSafetyMode.None;
                if (m_threadSafeObj == (object)PUBLICATION_ONLY_OR_ALREADY_INITIALIZED) return LazyThreadSafetyMode.PublicationOnly;
                return LazyThreadSafetyMode.ExecutionAndPublication;
            }
        }

        internal bool IsValueFaulted
        {
            get { return m_boxed is LazyInternalExceptionHolder; }
        }

        public bool IsValueCreated
        {
            get
            {
                return m_boxed != null && m_boxed is Boxed;
            }
        }

        public T Value
        {
            get
            {
                Boxed boxed = null;
                if (m_boxed != null)
                {
                    // Do a quick check up front for the fast path.
                    boxed = m_boxed as Boxed;
                    if (boxed != null)
                    {
                        return boxed.m_value;
                    }

                    LazyInternalExceptionHolder exc = m_boxed as LazyInternalExceptionHolder;
                    throw exc.m_exception;
                }

                return LazyInitValue();
            }
        }

        private T LazyInitValue()
        {
            Boxed boxed = null;
            LazyThreadSafetyMode mode = Mode;
            if (mode == LazyThreadSafetyMode.None)
            {
                boxed = CreateValue();
                m_boxed = boxed;
            }
            else if (mode == LazyThreadSafetyMode.PublicationOnly)
            {
                boxed = CreateValue();
                if (Interlocked.CompareExchange(ref m_boxed, boxed, null) != null)
                    boxed = (Boxed)m_boxed; // set the boxed value to the succeeded thread value
            }
            else
            {
                lock (m_threadSafeObj)
                {
                    if (m_boxed == null)
                    {
                        boxed = CreateValue();
                        m_boxed = boxed;
                    }
                    else // got the lock but the value is not null anymore, check if it is created by another thread or faulted and throw if so
                    {
                        boxed = m_boxed as Boxed;
                        if (boxed == null) // it is not Boxed, so it is a LazyInternalExceptionHolder
                        {
                            LazyInternalExceptionHolder exHolder = m_boxed as LazyInternalExceptionHolder;
                            throw exHolder.m_exception;
                        }
                    }
                }
            }
            return boxed.m_value;
        }

        private Boxed CreateValue()
        {
            Boxed boxed = null;
            LazyThreadSafetyMode mode = Mode;
            if (m_valueFactory != null)
            {
                try
                {
                    // check for recursion
                    if (mode != LazyThreadSafetyMode.PublicationOnly && m_valueFactory == PUBLICATION_ONLY_OR_ALREADY_INITIALIZED)
                        throw new InvalidOperationException();

                    Func<T> factory = m_valueFactory;
                    if (mode != LazyThreadSafetyMode.PublicationOnly) // only detect recursion on None and ExecutionAndPublication modes
                        m_valueFactory = PUBLICATION_ONLY_OR_ALREADY_INITIALIZED;
                    boxed = new Boxed(factory());
                }
                catch (Exception ex)
                {
                    if (mode != LazyThreadSafetyMode.PublicationOnly) // don't cache the exception for PublicationOnly mode
                    {
                    m_boxed = new LazyInternalExceptionHolder(ex);
                    }
                    throw;
                }
            }
            else
            {
                try
                {
                    boxed = new Boxed((T)Activator.CreateInstance(typeof(T)));

                }
                catch (System.MissingMethodException)
                {
                    Exception ex = new System.MissingMemberException("No parameterless constructor found for " + typeof(T).FullName);
                    if (mode != LazyThreadSafetyMode.PublicationOnly) // don't cache the exception for PublicationOnly mode
                        m_boxed = new LazyInternalExceptionHolder(ex);
                    throw ex;
                }
            }

            return boxed;
        }

    }
}
#endif
