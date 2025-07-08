using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
[assembly: AssemblyDelaySign(true)]
[assembly: Parallelize(Workers = 1, Scope = ExecutionScope.ClassLevel)]