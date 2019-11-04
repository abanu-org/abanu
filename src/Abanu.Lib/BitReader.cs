// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using Mosa.Runtime;

#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
#pragma warning disable CA1822 // Mark members as static

namespace Abanu.Kernel
{

    // required:
    //public static ref T AsRef<T>(void* source)
    //public static void Write<T>(void* destination, T value)
    //public static T Read<T>(void* source)

    // should work automatically:
    //public static int SizeOf<T>()
    //public static ref TTo As<TFrom, TTo>(ref TFrom source)
    //public static void* AsPointer<T>(ref T value)

    // May needed: ByReference<T>

    // Will provide: Span<T>, ReadonlySpan<T>, Memory<T> and much more cool stuff

    public class BitReader
    {
        private Pointer Addr;
        private uint Position;

        public BitReader(Pointer addr)
        {
            Addr = addr;
            Position = 0;
        }

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

        //public unsafe string ReadString()
        //{
        //    var s = (NullTerminatedString*)(Addr + Position);
        //    var resultString = NullTerminatedString.ToString(s);
        //    Position += (uint)resultString.Length + 1;
        //    return resultString;
        //}

        public unsafe T ReadStruct<T>()
            where T : struct
        {
            var size = Unsafe.SizeOf<T>();
            var value = Unsafe.Read<T>((void*)(Addr + Position));
            Position += (uint)size;
            return value;
        }

        public unsafe ref T ReadStructRef<T>()
        {
            var size = Unsafe.SizeOf<T>();
            ref T value = ref Unsafe.AsRef<T>((void*)(Addr + Position));
            Position += (uint)size;
            return ref value;
        }

    }

    // https://github.com/dotnet/coreclr/blob/master/src/System.Private.CoreLib/shared/System/Span.cs

    /// <summary>
    /// Span represents a contiguous region of arbitrary memory. Unlike arrays, it can point to either managed
    /// or native memory, or to memory allocated on the stack. It is type- and memory-safe.
    /// </summary>
    public ref struct Span<T>
    {

        private Pointer Addr;
        private int ElementSize;
        private int Elements;

        public Span(Pointer addr, int elements)
        {
            Addr = addr;
            Elements = elements;
            ElementSize = Unsafe.SizeOf<T>();
        }

        public unsafe ref T this[uint index]
        {
            get
            {
                if (index >= Elements)
                    throw new IndexOutOfRangeException();

                return ref Unsafe.AsRef<T>((void*)(Addr + (index * ElementSize)));
            }
            //set
            //{
            //    if (index >= Elements)
            //        throw new IndexOutOfRangeException();

            //    Unsafe.Write<T>((void*)(Addr + (index * ElementSize)), value);
            //}
        }

    }

    // https://github.com/dotnet/coreclr/blob/master/src/System.Private.CoreLib/shared/Internal/Runtime/CompilerServices/Unsafe.cs

    // The implementations of most the methods in this file are provided as intrinsics.
    // In CoreCLR, the body of the functions are replaced by the EE with unsafe code. See see getILIntrinsicImplementationForUnsafe for details.

    internal static unsafe class Unsafe
    {
        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from the given location.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //[Intrinsic]
        public static T Read<T>(void* source)
        {
            //return Unsafe.As<byte, T>(ref *(byte*)source);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the size of an object of the given type parameter.
        /// </summary>
        //[Intrinsic]
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
        //[Intrinsic]
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
        //[Intrinsic]
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

    // https://github.com/dotnet/coreclr/blob/master/src/System.Private.CoreLib/shared/System/ByReference.cs

    // ByReference<T> is meant to be used to represent "ref T" fields. It is working
    // around lack of first class support for byref fields in C# and IL. The JIT and
    // type loader has special handling for it that turns it into a thin wrapper around ref T.
    internal readonly ref struct ByReference<T>
    {
#pragma warning disable CA1823, 169 // private field '{blah}' is never used
        private readonly IntPtr _value;
#pragma warning restore CA1823, 169

        //[Intrinsic]
        public ByReference(ref T value)
        {
            // Implemented as a JIT intrinsic - This default implementation is for
            // completeness and to provide a concrete error if called via reflection
            // or if intrinsic is missed.
            throw new PlatformNotSupportedException();
        }

        public ref T Value
        {
            // Implemented as a JIT intrinsic - This default implementation is for
            // completeness and to provide a concrete error if called via reflection
            // or if the intrinsic is missed.
            //[Intrinsic]
            get => throw new PlatformNotSupportedException();
        }
    }

    // -------  TESTS ---------------

    public struct TestStruct
    {
        public int Member1;
        public long Member2;
        public TestSubStruct Member3;
    }

    public struct TestSubStruct
    {
        public bool Member4;
    }

    public static class StructTest
    {
        public static void Test()
        {

            var reader = new BitReader((Pointer)0x2000);

            // by value
            var valueStruct = reader.ReadStruct<TestStruct>();
            valueStruct.Member2 = 88;

            // by ref
            ref var refStruct = ref reader.ReadStructRef<TestStruct>();
            refStruct.Member2 = 88;

            // span test
            var span = new Span<TestStruct>();
            span[3].Member2 = 88;

            // by value
            valueStruct = span[3];

            // by ref
            refStruct = ref span[3];
            refStruct.Member2 = 88;

        }
    }

}
