#if SILVERLIGHT || CLR35
// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;

// Just to allow us to compile in 3.5 and SL with minimal code changes

namespace System.Diagnostics.Contracts
{

    internal class ContractArgumentValidatorAttribute : Attribute
    {
    }

    internal static class Contract
    {
        public static void Ensures(bool expressionResult)
        {
        }

        public static bool ForAll<T>(IEnumerable<T> values, Func<T, bool> exp)
        {
            foreach (var value in values)
            {
                if (!exp(value))
                {
                    return false;
                }
            }
            return true;
        }

        public static T Result<T>() 
        {
            return default(T);
        }

        public static void EndContractBlock()
        {
        }
    }
}
#endif