// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Abanu;
using Abanu.Runtime;
using Mosa.Runtime;

namespace System.Threading
{

    public class Thread
    {

        private unsafe ThreadLocalStorageBlock* TLS;
        private int _ManagedThreadId;

        internal unsafe Thread(ThreadLocalStorageBlock* tls)
        {
            TLS = tls;
            _ManagedThreadId = tls->ThreadID;
        }

        public int ManagedThreadId => _ManagedThreadId;

        public static unsafe Thread Current
        {
            get
            {
                var threadPtr = (Pointer)ApplicationRuntime.GetThreadLocalStorageBlock()->ThreadPtr;
                return (Thread)Intrinsic.GetObjectFromAddress(threadPtr);
            }
        }

        public static void Sleep(int millisecondsTimeout)
        {
            SysCalls.ThreadSleep((uint)millisecondsTimeout);
        }

        public static unsafe object GetData(LocalDataStoreSlot slot)
        {
            var value = GetThreadLocalStorage(slot.Position);
            return Intrinsic.GetObjectFromAddress((Pointer)value);
        }

        public static unsafe void SetData(LocalDataStoreSlot slot, object data)
        {
            var value = Intrinsic.GetObjectAddress(data);
            SetThreadLocalStorage(slot.Position, (void*)value);
        }

        public static LocalDataStoreSlot AllocateNamedDataSlot(string name)
        {
            // TODO
            return AllocateDataSlot(Addr.Size);
        }

        private static LocalDataStoreSlot AllocateDataSlot(int size)
        {
            var pos = NextSlotPosition;
            NextSlotPosition += size;
            return new LocalDataStoreSlot(NextSlotPosition);
        }

        internal static int NextSlotPosition = 0;

        [DllImport("x86/Abanu.GetThreadLocalStorage.o", EntryPoint = "GetThreadLocalStorage")]
        internal static extern unsafe void* GetThreadLocalStorage(int index);

        [DllImport("x86/Abanu.SetThreadLocalStorage.o", EntryPoint = "SetThreadLocalStorage")]
        internal static extern unsafe int SetThreadLocalStorage(int index, void* value);

    }

}
