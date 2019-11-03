// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using Mosa.Runtime;

namespace Abanu.Kernel
{
    public class BitReader
    {
        private Pointer Addr;
        private uint Position;

        public byte ReadByte()
        {
            var value = Intrinsic.Load8(Addr, Position);
            Position += 1;
            return value;
        }

        public sbyte ReadSByte()
        {
            var value = Intrinsic.Load8(Addr, Position);
            Position += 1;
            return (sbyte)value;
        }

        public int ReadInt32()
        {
            var value = Intrinsic.Load32(Addr, Position);
            Position += 4;
            return (int)value;
        }

        public uint ReadUInt32()
        {
            var value = Intrinsic.Load32(Addr, Position);
            Position += 4;
            return value;
        }

        public long ReadInt64()
        {
            var value = Intrinsic.Load64(Addr, Position);
            Position += 4;
            return (int)value;
        }

        public ulong ReadUInt64()
        {
            var value = Intrinsic.Load64(Addr, Position);
            Position += 4;
            return value;
        }

        public unsafe string ReadString()
        {
            var s = (NullTerminatedString*)(Addr + Position);
            var resultString = NullTerminatedString.ToString(s);
            Position += (uint)resultString.Length + 1;
            return resultString;
        }

        public unsafe T ReadStruct<T>()
            where T : struct
        {
            var size = Unsafe.SizeOf<T>();
            var value = Unsafe.Read<T>((void*)(Addr + Position));
            Position += (uint)size;
            return value;
        }

    }

    internal static unsafe class Unsafe
    {
        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from the given location.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Read<T>(void* source)
        {
            //return Unsafe.As<byte, T>(ref *(byte*)source);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the size of an object of the given type parameter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizeOf<T>()
        {
            throw new PlatformNotSupportedException();

            // sizeof !!0
            // ret
        }

        /// <summary>
        /// Reinterprets the given reference as a reference to a value of type <typeparamref name="TTo"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TTo As<TFrom, TTo>(ref TFrom source)
        {
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ret
        }

        /// <summary>
        /// Reinterprets the given location as a reference to a value of type <typeparamref name="T"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(void* source)
        {
            return ref Unsafe.As<byte, T>(ref *(byte*)source);
        }

        /// <summary>
        /// Returns a pointer to the given by-ref parameter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AsPointer<T>(ref T value)
        {
            throw new PlatformNotSupportedException();

            // ldarg.0
            // conv.u
            // ret
        }

        /// <summary>
        /// Writes a value of type <typeparamref name="T"/> to the given location.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write<T>(void* destination, T value)
        {
            Unsafe.As<byte, T>(ref *(byte*)destination) = value;
        }

    }

}
