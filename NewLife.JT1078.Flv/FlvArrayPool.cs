using System.Buffers;

namespace NewLife.JT1078.Flv
{
    internal static class FlvArrayPool
    {
        private static readonly ArrayPool<System.Byte> ArrayPool;

        static FlvArrayPool() => ArrayPool = ArrayPool<System.Byte>.Create();

        public static System.Byte[] Rent(System.Int32 minimumLength) => ArrayPool.Rent(minimumLength);

        public static void Return(System.Byte[] array, System.Boolean clearArray = false) => ArrayPool.Return(array, clearArray);
    }
}
