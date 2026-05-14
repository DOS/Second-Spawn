// SECOND SPAWN PATCH (2026-05-14): grant CLR access bypass for Fusion.Runtime
// internals from this assembly. Required because Fusion.Runtime.dll's
// [InternalsVisibleTo] declarations (Fusion.Unity.Tests/Editor, Unity.Fusion.CodeGen,
// Fusion.Plugin/Json/Runtime.Tests) do not include SecondSpawn.Networking, but
// Fusion's IL weaver emits direct accesses to Fusion.NetworkBehaviour::Ptr
// (internal field) inside every [Networked] property getter/setter. Without
// this attribute the CLR raises FieldAccessException at runtime as soon as
// NetworkPlayer.Spawned() runs against a user-asmdef-hosted NetworkBehaviour.
//
// IgnoresAccessChecksToAttribute is a Roslyn/CLR convention: when the compiler
// sees this attribute on the calling assembly, it emits IL that skips the
// runtime access check for the named target. Honored by .NET Core 3+, Mono
// 5.10+, and Unity 6's bundled Mono runtime. The attribute type is not in the
// BCL, so we declare it ourselves below; Roslyn matches by full name.
//
// Long-term fix: when Photon ships a Fusion version that grants this access
// automatically via AssembliesToWeave (or exposes Ptr as public), remove this
// file. Track release_history.txt; revert this workaround when confirmed.

[assembly: System.Runtime.CompilerServices.IgnoresAccessChecksTo("Fusion.Runtime")]

namespace System.Runtime.CompilerServices
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
    internal sealed class IgnoresAccessChecksToAttribute : System.Attribute
    {
        public IgnoresAccessChecksToAttribute(string assemblyName)
        {
            AssemblyName = assemblyName;
        }

        public string AssemblyName { get; }
    }
}
