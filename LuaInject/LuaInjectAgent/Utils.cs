using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using EasyHook;

namespace LuaInjectAgent
{
    public class Utils
    {
        public static void TryNames<TDelegate>(string function, Action<string> action)
        {
            try
            {
                action(function);
            }
            catch
            {
                var decoratedName = DecoratedName<TDelegate>(function);
                try
                {
                    action(decoratedName);
                }
                catch
                {
                    throw;
                }
            }
        }

        public static TDelegate GetProcDelegate<TDelegate>(string hookedModule, string function)
        {
            TDelegate result = default(TDelegate);

            TryNames<TDelegate>(function, (name) => result = LocalHook.GetProcDelegate<TDelegate>(hookedModule, name));

            return result;
        }

        public static LocalHook SetupHook<TDelegate>(string function, string hookedModule, Func<TDelegate, TDelegate> hookProcedure, object callback = null)
        {

            LocalHook hook = null;

            TryNames<TDelegate>(function, (name) => hook = LocalHook.Create(
                    LocalHook.GetProcAddress(hookedModule, name),
                    hookProcedure(LocalHook.GetProcDelegate<TDelegate>(hookedModule, name)) as Delegate,
                    callback)
            );

            hook.ThreadACL.SetExclusiveACL(new[] { 0 });

            return hook;
        }

        // TODO: add some typical error handling
        private static T DynamicDllInvoke<T>(string functionName, string library)
        {
            // create in-memory assembly, module and type
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("DynamicDllInvoke"),
                AssemblyBuilderAccess.Run);

            ModuleBuilder modBuilder = assemblyBuilder.DefineDynamicModule("DynamicDllModule");

            // note: without TypeBuilder, you can create global functions
            // on the module level, but you cannot create delegates to them
            TypeBuilder typeBuilder = modBuilder.DefineType(
                "DynamicDllInvokeType",
                TypeAttributes.Public | TypeAttributes.UnicodeClass);

            // get params from delegate dynamically (!), trick from Eric Lippert
            MethodInfo delegateMI = typeof(T).GetMethod("Invoke");
            Type[] delegateParams = (from param in delegateMI.GetParameters()
                                     select param.ParameterType).ToArray();

            // automatically create the correct signagure for PInvoke
            MethodBuilder methodBuilder = typeBuilder.DefinePInvokeMethod(
                functionName,
                library,
                MethodAttributes.Public |
                MethodAttributes.Static |
                MethodAttributes.PinvokeImpl,
                CallingConventions.Standard,
                delegateMI.ReturnType,        /* the return type */
                delegateParams,               /* array of parameters from delegate T */
                CallingConvention.Winapi,
                CharSet.Ansi);

            // needed according to MSDN
            methodBuilder.SetImplementationFlags(
                methodBuilder.GetMethodImplementationFlags() |
                MethodImplAttributes.PreserveSig);

            Type dynamicType = typeBuilder.CreateType();

            MethodInfo methodInfo = dynamicType.GetMethod(functionName);

            // create the delegate of type T, double casting is necessary
            return (T)(object)Delegate.CreateDelegate(
                typeof(T),
                methodInfo, true);
        }


        // TODO: add some typical error handling
        private static string DecoratedName<T>(string functionName)
        {
            // create in-memory assembly, module and type
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("MakePInvokeMethod"),
                AssemblyBuilderAccess.Run);

            ModuleBuilder modBuilder = assemblyBuilder.DefineDynamicModule("MakePInvokeMethodModule");

            // note: without TypeBuilder, you can create global functions
            // on the module level, but you cannot create delegates to them
            TypeBuilder typeBuilder = modBuilder.DefineType(
                "MakePInvokeMethodType",
                TypeAttributes.Public | TypeAttributes.UnicodeClass);

            // get params from delegate dynamically (!), trick from Eric Lippert
            MethodInfo delegateMI = typeof(T).GetMethod("Invoke");
            Type[] delegateParams = (from param in delegateMI.GetParameters()
                                     select param.ParameterType).ToArray();

            // automatically create the correct signagure for PInvoke
            MethodBuilder methodBuilder = typeBuilder.DefinePInvokeMethod(
                functionName,
                "foo",
                MethodAttributes.Public |
                MethodAttributes.Static |
                MethodAttributes.PinvokeImpl,
                CallingConventions.Standard,
                delegateMI.ReturnType,        /* the return type */
                delegateParams,               /* array of parameters from delegate T */
                CallingConvention.Winapi,
                CharSet.Ansi);

            // needed according to MSDN
            methodBuilder.SetImplementationFlags(
                methodBuilder.GetMethodImplementationFlags() |
                MethodImplAttributes.PreserveSig);

            Type dynamicType = typeBuilder.CreateType();

            MethodInfo methodInfo = dynamicType.GetMethod(functionName);

            return string.Format("@{0}@{1}", functionName, Marshal.NumParamBytes(methodInfo));
        }
    }
}
