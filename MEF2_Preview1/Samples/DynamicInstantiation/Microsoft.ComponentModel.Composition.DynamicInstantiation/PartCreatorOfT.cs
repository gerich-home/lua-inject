// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;

namespace Microsoft.ComponentModel.Composition.DynamicInstantiation
{
    public class PartCreator<T>
    {
        private readonly Func<PartLifetimeContext<T>> _creator;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public PartCreator(Func<PartLifetimeContext<T>> creator)
        {
            if (creator == null) throw new ArgumentNullException("creator");
            this._creator = creator;
        }

        public PartLifetimeContext<T> CreatePart()
        {
            return this._creator();
        }
    }
}