using System.Diagnostics.CodeAnalysis;
using ToyORB;

namespace {namespace}
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface I{interfaceName} : IToyOrbService
    {
#pragma warning disable IDE1006 // Naming Styles
        {methodDeclarations}
#pragma warning restore IDE1006 // Naming Styles
    }
}
