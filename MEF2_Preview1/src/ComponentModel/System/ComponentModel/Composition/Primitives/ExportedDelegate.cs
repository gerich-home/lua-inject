// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Internal;
using System.Linq.Expressions;

namespace System.ComponentModel.Composition.Primitives
{
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class ExportedDelegate
    {
        private object _instance;
        private MethodInfo _method;

        protected ExportedDelegate() { }
#if !SILVERLIGHT
        [System.Security.SecurityCritical]
#endif
#if CLR35
        [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name = "FullTrust")]
#endif
        public ExportedDelegate(object instance, MethodInfo method)
        {
            Requires.NotNull(method, "method");

            this._instance = instance;
            this._method = method;
        }

        public virtual Delegate CreateDelegate(Type delegateType) 
        {
            Requires.NotNull(delegateType, "delegateType");

            if (delegateType == typeof(Delegate) || delegateType == typeof(MulticastDelegate))
            {
#if SILVERLIGHT || CLR35
                Type funcOrAction = ConvertMethodInfoToFuncOrActionType(this._method);

                if (funcOrAction != null)
                {
                    delegateType = funcOrAction;
                }
                else
                {
                    return null;
                }
#else
                delegateType = this.CreateStandardDelegateType();
#endif
            }
            
            return Delegate.CreateDelegate(delegateType, this._instance, this._method, false);
        }

#if SILVERLIGHT || CLR35
private static Type[] _funcTypes = 
        { 
            typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>) 
#if CLR40 && !SILVERLIGHT
            , typeof(Func<,,,,,>), typeof(Func<,,,,,,>), typeof(Func<,,,,,,,>), typeof(Func<,,,,,,,,>)
#endif 
        };

        private static Type[] _actionTypes = 
        { 
            typeof(Action), typeof(Action<>), typeof(Action<,>), typeof(Action<,,>), typeof(Action<,,,>) 
#if CLR40 && !SILVERLIGHT
            , typeof(Action<,,,,>), typeof(Action<,,,,,>), typeof(Action<,,,,,,>), typeof(Action<,,,,,,,>)
#endif
        };

        private static Type ConvertMethodInfoToFuncOrActionType(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            bool isVoid = method.ReturnType == typeof(void);
            Type[] typeArray = isVoid ? _actionTypes : _funcTypes;

            if (parameters.Length >= typeArray.Length)
            {
                return null;
            }

            Type[] genericArgTypes = new Type[parameters.Length + (isVoid ? 0 : 1)];

            for (int i = 0; i < parameters.Length; i++)
            {
                genericArgTypes[i] = parameters[i].ParameterType;
            }

            if (!isVoid)
            {
                genericArgTypes[parameters.Length] = method.ReturnType;
            }

            Type delegateType = typeArray[parameters.Length].IsGenericType ?
                typeArray[parameters.Length].MakeGenericType(genericArgTypes) :
                typeArray[parameters.Length];

            return delegateType;
        }
#else
        private Type CreateStandardDelegateType()
        {
            ParameterInfo[] parameters = this._method.GetParameters();

            // This array should contains a lit of all argument types, and the last one is the return type (could be void)
            Type[] parameterTypes = new Type[parameters.Length + 1];
            parameterTypes[parameters.Length] = this._method.ReturnType;
            for (int i = 0; i < parameters.Length; i++ )
            {
                parameterTypes[i] = parameters[i].ParameterType;
            }

            return Expression.GetDelegateType(parameterTypes);
        }
#endif
    }
}
