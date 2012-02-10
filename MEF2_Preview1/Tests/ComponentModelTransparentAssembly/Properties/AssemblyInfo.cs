// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Reflection;
using System.Security;

#if CLR35
[assembly: SecurityCritical(SecurityCriticalScope.Everything)]
#else
[assembly: SecurityTransparent]
#endif

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: CLSCompliant(true)]
