using Mosa.Runtime.x86;
using System;
using Mosa.Kernel.x86;

namespace lonos.kernel.core
{

    public unsafe static class PageDump
    {

        public static void Dump()
        {
            var pages = PageTable.TotalPhysPages;
            int col = 0;
            for (uint page = 0; page < pages; page++)
            {
                var e = PageTable.GetTableEntryByIndex(page);
                var p = PageFrameManager.GetPageByIndex(page);
                var c = GetChar(e, p);
                Devices.Serial1.Write(c);
            }
        }

        private static char GetChar(PageTable.PageTableEntry* entry, Page* page)
        {
            //if(page->)
            return ' ';
        }

    }

}
