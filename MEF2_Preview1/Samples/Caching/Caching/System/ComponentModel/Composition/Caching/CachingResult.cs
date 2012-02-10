// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Internal.Collections;
using System.Text;
using System.Globalization;

namespace System.ComponentModel.Composition.Caching
{
    internal struct CachingResult
    {
        public static readonly CachingResult SucceededResult = new CachingResult();
        private readonly IEnumerable<string> _errors;

        public CachingResult(params string[] errors)
            : this((IEnumerable<string>)errors)
        {            
        }

        public CachingResult(IEnumerable<string> errors)
        {
            _errors = errors;
        }

        public bool Succeeded
        {
            get { return this._errors == null || !this._errors.Any(); }
        }

        public IEnumerable<string> Errors
        {
            get { return this._errors ?? Enumerable.Empty<string>(); }
        }

        public CachingResult MergeResult(CachingResult result)
        {
            return MergeErrors(result._errors);
        }

        public CachingResult MergeError(string error)
        {
            return MergeErrors(new string[] { error });
        }

        public CachingResult MergeError(string format, params object[] args)
        {
            return MergeError(string.Format(CultureInfo.InvariantCulture, format, args));
        }

        public CachingResult MergeErrors(IEnumerable<string> errors)
        {
            if (errors == null)
            {
                return new CachingResult(this._errors);
            }
            else if (this._errors == null)
            {
                return new CachingResult(errors);
            }
            else
            {
                return new CachingResult(this._errors.Concat(errors));
            }
        }

        public CachingResult<T> ToResult<T>(T value)
        {
            return new CachingResult<T>(value, this._errors); 
        }

        public void ThrowOnErrors()
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
