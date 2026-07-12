using System;
using System.Runtime.InteropServices;

namespace NewLife.JT1078.Flv.Extensions
{
    public static class InteropExtensions
    {
        public static T BytesToStruct<T>(Byte[] bytes, Int32 startIndex, Int32 length)
        {
            T local;
            T local2;
            if (bytes == null)
            {
                local2 = default;
                local = local2;
            }
            else if (bytes.Length <= 0)
            {
                local2 = default;
                local = local2;
            }
            else
            {
                var destination = Marshal.AllocHGlobal(length);
                try
                {
                    Marshal.Copy(bytes, startIndex, destination, length);
                    local = (T)Marshal.PtrToStructure(destination, typeof(T));
                }
                catch (Exception exception)
                {
                    throw new Exception("Error in BytesToStruct ! " + exception.Message);
                }
                finally
                {
                    Marshal.FreeHGlobal(destination);
                }
            }
            return local;
        }

        public static void IntPtrSetValue(IntPtr intptr, Object structObj) => IntPtrSetValue(intptr, StructToBytes(structObj));

        public static void IntPtrSetValue(IntPtr intptr, Byte[] bytes) => Marshal.Copy(bytes, 0, intptr, bytes.Length);

        public static T IntPtrToStruct<T>(IntPtr intptr)
        {
            var index = 0;
            return IntPtrToStruct<T>(intptr, index, Marshal.SizeOf(typeof(T)));
        }

        public static T IntPtrToStruct<T>(IntPtr intptr, Int32 index, Int32 length)
        {
            var destination = new Byte[length];
            Marshal.Copy(intptr, destination, index, length);
            return BytesToStruct<T>(destination, 0, destination.Length);
        }

        public static Byte[] StructToBytes(Object structObj)
        {
            var size = Marshal.SizeOf(structObj);
            return StructToBytes(structObj, size);
        }

        public static Byte[] StructToBytes(Object structObj, Int32 size)
        {
            Byte[] buffer2;
            var ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structObj, ptr, false);
                var destination = new Byte[size];
                Marshal.Copy(ptr, destination, 0, size);
                buffer2 = destination;
            }
            catch (Exception exception)
            {
                throw new Exception("Error in StructToBytes ! " + exception.Message);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return buffer2;
        }
    }
}
