// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Internal.Collections;
using System.Text;

namespace System.ComponentModel.Composition.Caching
{
    internal struct CachingResult<T>
    {
        private readonly IEnumerable<string> _errors;
        private readonly T _value;
        
        public CachingResult(T value)
            : this(value, (string[])null)
        {
        }

        public CachingResult(params string[] errors)
            : this(default(T), (IEnumerable<string>)errors)
        {
        }

        public CachingResult(IEnumerable<string> errors)
            : this(default(T), errors)
        {
        }

        internal CachingResult(T value, IEnumerable<string> errors)
        {
            this._errors = errors;
            this._value = value;
        }

        public bool Succeeded
        {
            get { return this._errors == null || !this._errors.Any(); }
        }

        public IEnumerable<string> Errors
        {
            get { return this._errors ?? Enumerable.Empty<string>(); }
        }

        /// <summary>
        ///     Gets the value from the result, throwing a CompositionException if there are any errors.
        /// </summary>
        public T Value
        {
            get 
            {
                ThrowOnErrors();

                return this._value; 
            }
        }

        internal CachingResult<TValue> ToResult<TValue>()
        {
            return new CachingResult<TValue>(this._errors);
        }

        internal CachingResult ToResult()
        {
            return new CachingResult(this._errors);
        }

        private void ThrowOnErrors()
        {
            if (!this.Succeeded)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Strings.CachingResult_ExceptionMessageHeader);
                foreach (string error in this.Errors)
                {
                    sb.AppendLine(error);
                }

                throw new InvalidOperationException(sb.ToString());
            }
        }
    }
}
