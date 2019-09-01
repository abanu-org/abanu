using lonos.Kernel.Core.Collections;
using lonos.Kernel.Core.Interrupts;
using lonos.Kernel.Core.MemoryManagement;
using lonos.Kernel.Core.PageManagement;
using lonos.Kernel.Core.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lonos.Kernel.Core.SysCalls
{
    public unsafe class SysCallManager
    {

        public const uint IRQ = 250;

        public static void Setup()
        {
            KernelMessage.WriteLine("Initialize SysCall Manager");

            IDTManager.SetInterruptHandler(IRQ, InterruptHandler);
            IDTManager.SetPrivilegeLevel(IRQ, 0x03);
            IDTManager.Flush();

            Commands = new SysCallInfo[256];
            SetCommands();

            // hack!!
            nextVirtPage = 300 * 1024 * 1024;
        }

        private static void SetCommands()
        {
            SetCommand(KnownSysCallCommand.RequestPages, cmd_RequestPage);
        }


        private static uint nextVirtPage;
        private static uint cmd_RequestPage(uint arg0)
        {
            var pages = arg0;
            var page = PageFrameManager.AllocatePages(PageFrameRequestFlags.Default, pages);
            var virtAddr = nextVirtPage;
            nextVirtPage += (pages * 4096);
            Scheduler.GetCurrentThread().Process.PageTable.Map(nextVirtPage, page->PhysicalAddress, pages * 4096);
            return virtAddr;
        }

        public static void SetCommand(KnownSysCallCommand command, DSysCallInfoHandler handler)
        {
            Commands[(uint)command] = new SysCallInfo
            {
                CommandID = command,
                Handler = handler,
            };
        }

        public static void InterruptHandler(IDTStack* stack)
        {
            KernelMessage.WriteLine("Got SysCall {0}, arg0={1}", stack->EAX, stack->ECX);
            stack->EAX = Commands[stack->EAX].Handler(stack->ECX);
        }

        private static SysCallInfo[] Commands;
    }

    public delegate uint DSysCallInfoHandler(uint arg0);

    public enum KnownSysCallCommand
    {
        RequestPages = 20
    }

    public class SysCallInfo
    {
        public KnownSysCallCommand CommandID;
        //public uint Arguments;
        //public string Name;
        public DSysCallInfoHandler Handler;
    }

}
