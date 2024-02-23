using System;
using System.Runtime.CompilerServices;

namespace FluentIcons.Common.Internals
{
    internal static class SymbolConversion
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ToString(this Symbol symbol, bool isFilled, bool isRtl)
            => char.ConvertFromUtf32((int)symbol + Convert.ToInt32(isFilled) + (Convert.ToInt32(isRtl) << 1));
    }
}
