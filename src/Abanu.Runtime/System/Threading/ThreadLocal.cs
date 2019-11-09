// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

namespace System.Threading
{
    public class ThreadLocal<T>
    {

        private LocalDataStoreSlot Slot;
        private LocalDataStoreSlot SlotIsInitialized;

        public ThreadLocal()
        {
            Slot = Thread.AllocateNamedDataSlot("");
            SlotIsInitialized = Thread.AllocateNamedDataSlot("");
        }

        public T Value
        {
            get
            {
                return (T)Thread.GetData(Slot);
            }
            set
            {
                Thread.SetData(Slot, value);
                IsValueCreated = true;

            }
        }

        public bool IsValueCreated
        {
            get
            {
                return (bool)Thread.GetData(SlotIsInitialized);
            }
            private set
            {
                Thread.SetData(SlotIsInitialized, true);
            }
        }

    }

}
