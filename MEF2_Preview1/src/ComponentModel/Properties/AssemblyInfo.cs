// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Runtime.CompilerServices;
using System.Security;

#if !SILVERLIGHT
[assembly: AllowPartiallyTrustedCallers]
#endif

#if CLR35
[assembly: SecurityCritical(SecurityCriticalScope.Everything)]
#endif

[assembly: System.CLSCompliant(true)]
[assembly: System.Reflection.AssemblyTitle("System.ComponentModel.Composition")]
[assembly: System.Reflection.AssemblyCopyright("(c) Microsoft Corporation. All rights reserved.")]
[assembly: System.Reflection.AssemblyDescription("Codeplex version (http://mef.codeplex.com)")]
[assembly: System.Reflection.AssemblyVersion("4.0.0.1")]
[assembly: System.Reflection.AssemblyFileVersion("1.0.0.0")]

[assembly: InternalsVisibleTo("System.ComponentModel.Composition.UnitTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100616470ad6a034af669d130b58deedb7ad8544920d8a21d95bc5bb535ca673d8a49b228c5163f78f34b8df3b015fc2b99ff45b7536830a596f711b8b09f80b48a4bf20883ee5b97f50462d7e0f33440f024dae7d8f7eaf875b747619f1e772131a24dea9d5f80e5d54d95f0704f78fe84ac4b3774ce17eb00a764c295846d43e3")]
[assembly: InternalsVisibleTo("System.ComponentModel.Composition.UnitTestFramework, PublicKey=0024000004800000940000000602000000240000525341310004000001000100616470ad6a034af669d130b58deedb7ad8544920d8a21d95bc5bb535ca673d8a49b228c5163f78f34b8df3b015fc2b99ff45b7536830a596f711b8b09f80b48a4bf20883ee5b97f50462d7e0f33440f024dae7d8f7eaf875b747619f1e772131a24dea9d5f80e5d54d95f0704f78fe84ac4b3774ce17eb00a764c295846d43e3")]

