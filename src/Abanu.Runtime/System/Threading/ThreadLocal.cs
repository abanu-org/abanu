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
