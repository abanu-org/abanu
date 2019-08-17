using System;
namespace lonos.kernel.core
{
    public unsafe static class BootInfo
    {

        public unsafe static BootInfoHeader* Header;

        public static bool Present;

        public static void SetupStage1()
        {
            Header = (BootInfoHeader*)Address.KernelBootInfo;
            ApplyAddresses();
        }

        public static void SetupStage2()
        {
            if (Header->Magic != BootInfoHeader.BootInfoMagic)
            {
                Present = false;
                KernelMessage.WriteLine("bootinfo not present");
                return;
            }

            Present = true;
            KernelMessage.WriteLine("bootinfo present");

            var mapLen = Header->MemoryMapLength;
            KernelMessage.WriteLine("Maps: {0}", mapLen);
            for (uint i = 0; i < mapLen; i++)
            {
                var mm = Header->MemoryMapArray[i];
                KernelMessage.WriteLine("Map Start={0:X8}, Size={1:X8}, Type={2}", mm.Start, mm.Size, (uint)mm.Type);
            }
        }

        // static void ApplyAddresses()
        // {
        //     GDT.KernelSetup(GetAddrOfMapType(BootInfoMemoryType.GDT));
        //     PageTable.KernelSetup(
        //         GetAddrOfMapType(BootInfoMemoryType.PageDirectory),
        //         GetAddrOfMapType(BootInfoMemoryType.PageTable));
        // }

        // static Addr GetAddrOfMapType(BootInfoMemoryType type)
        // {
        //     var mapLen = Header->MemoryMapLength;
        //     //KernelMessage.WriteLine("Maps: {0}", mapLen);
        //     for (uint i = 0; i < mapLen; i++)
        //     {
        //         if (Header->MemoryMapArray[i].Type == type)
        //             return Header->MemoryMapArray[i].Start;
        //     }
        //     return Addr.Zero;
        // }

        static void ApplyAddresses()
        {
            GDT.KernelSetup(GetMap(BootInfoMemoryType.GDT)->Start);
            PageTable.KernelSetup(
                GetMap(BootInfoMemoryType.PageDirectory)->Start,
                GetMap(BootInfoMemoryType.PageTable)->Start);
        }

        public static BootInfoMemory* GetMap(BootInfoMemoryType type)
        {
            var mapLen = Header->MemoryMapLength;
            //KernelMessage.WriteLine("Maps: {0}", mapLen);
            for (uint i = 0; i < mapLen; i++)
            {
                if (Header->MemoryMapArray[i].Type == type)
                    return &Header->MemoryMapArray[i];
            }
            return null;
        }

    }
}
