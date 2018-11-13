using System;
using Mosa.Kernel.x86;
using Mosa.Runtime.x86;

namespace lonos.kernel.core
{

	public enum GFP
	{
		GFP_ATOMIC,
		GFP_KERNEL,
		GFP_KERNEL_ACCOUNT,
		GFP_NOWAIT,
		GFP_NOIO,
		GFP_NOFS,
		GFP_USER,
		GFP_DMA,
		GFP_DMA32,
		GFP_HIGHUSER,
		GFP_HIGHUSER_MOVABLE,
		GFP_TRANSHUGE_LIGHT,
		GFP_TRANSHUGE,
		GFP_MOVABLE_MASK,
		GFP_MOVABLE_SHIFT,
	}



	public static class Memory
	{

		private static UIntPtr FirstAvailableAdress;

		public static void Init()
		{
			//FirstAvailableAdress = (UIntPtr)0x00B00000; //12M
			FirstAvailableAdress = (UIntPtr)(Multiboot.GetMemoryMapBase(Multiboot.MemoryMapCount)-Multiboot.GetMemoryMapLength(Multiboot.MemoryMapCount));

			for (uint index = 0; index < Multiboot.MemoryMapCount; index++)
			{
				var type = Multiboot.GetMemoryMapType(index);
				var start = Multiboot.GetMemoryMapBase(index);
				var size = Multiboot.GetMemoryMapLength(index);
			}

			//ClearMemory();
			GDT.Setup();

		}

		private unsafe static void ClearMemory()
		{
			var start = (uint)FirstAvailableAdress;
			//var end = (uint)Multiboot.MemoryUpper*1024;
			var end = start +1000;
			var ptr = start;
			uint i = 0;

			while (ptr < end)
			{
				uint* addr = (uint*)ptr;
				//*addr = 0;
				i++;
				//Boot.RawWrite(0, i+1, 'X', 2);
				ptr += 4;
			}

		}

		/// <summary>
		/// kmalloc is the normal method of allocating memory for objects smaller than page size in the kernel.
		/// </summary>
		public static UIntPtr kmalloc(USize n, GFP flags)
		{
			return UIntPtr.Zero;
		}

		/// <summary>
		/// allocate memory. The memory is set to zero.
		/// </summary>
		public static UIntPtr kzalloc(USize n, GFP flags)
		{
			return UIntPtr.Zero;
		}

		/// <summary>
		/// allocate memory for an array.
		/// </summary>
		public static UIntPtr kmalloc_array(USize elements, USize size, GFP flags)
		{
			return UIntPtr.Zero;
		}

		/// <summary>
		/// allocate memory for an array. The memory is set to zero.
		/// </summary>
		public static UIntPtr kcalloc(USize elements, USize size, GFP flags)
		{
			return UIntPtr.Zero;
		}

		/// <summary>
		/// 
		/// </summary>
		public static UIntPtr vmalloc(USize size, GFP flags)
		{
			return UIntPtr.Zero;
		}

		/// <summary>
		/// free previously allocated memory
		/// </summary>
		public static void kfree(UIntPtr address)
		{
		}

		/// <summary>
		/// release memory allocated by vmalloc()
		/// </summary>
		public static void vfree(UIntPtr address)
		{
		}

		/*
		void* kmem_cache_alloc(struct kmem_cache * cachep, gfp_t flags)
		void kmem_cache_free(struct kmem_cache * cachep, void* objp)
        */

		/// <summary>
        /// Clears the specified memory area.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="bytes">The bytes.</param>
        public static void Clear(uint start, uint bytes)
        {
            if (bytes % 4 == 0)
            {
                Clear4(start, bytes);
                return;
            }

            for (uint at = start; at < (start + bytes); at++)
                Native.Set8(at, 0);
        }

        public static void Clear4(uint start, uint bytes)
        {
            for (uint at = start; at < (start + bytes); at = at + 4)
                Native.Set32(at, 0);
        }

        public static void Copy(uint source, uint destination, uint length)
        {
            for (uint i = 0; i < length; i++)
                Native.Set8(destination + i, Native.Get8(source + i));  //TODO: Optimize with Set32
        }

	}

}
