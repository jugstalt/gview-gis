using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace gView.Framework.system.Diagnostics
{
    static public class DiagnosticParametersExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Task<T> StopIdleAsync<T>(this Task<T> asyncTask, DiagnosticParameters diagnostics)
            => diagnostics == null ?
                asyncTask :
                diagnostics.StopIdleTimeAsync(asyncTask);
    }
}
