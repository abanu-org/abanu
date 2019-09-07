using lonos.Kernel.Core.Collections;
using lonos.Kernel.Core.Interrupts;
using lonos.Kernel.Core.MemoryManagement;
using lonos.Kernel.Core.PageManagement;
using lonos.Kernel.Core.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
            SetCommand(KnownSysCallCommand.RequestMemory, cmd_RequestMemory);
            SetCommand(KnownSysCallCommand.ServiceFunc1, cmd_CallServiceFunc1);
            SetCommand(KnownSysCallCommand.ServiceReturn, cmd_ServiceReturn);
        }


        private static uint nextVirtPage;
        private static uint cmd_RequestMemory(SystemMessage* args)
        {
            var pages = KMath.DivCeil(args->Arg1, 4096);
            var page = PageFrameManager.AllocatePages(PageFrameRequestFlags.Default, pages);
            var virtAddr = nextVirtPage;
            nextVirtPage += (pages * 4096);
            Scheduler.GetCurrentThread().Process.PageTable.Map(nextVirtPage, page->PhysicalAddress, pages * 4096);
            return virtAddr;
        }

        private static uint cmd_CallServiceFunc1(SystemMessage* args)
        {
            var serv = KernelStart.serv;

            //0xAABBCCDD

            //var servArgs = new ServiceArgs
            //{
            //    Arg1 = args.Arg2,
            //    Arg2 = args.Arg3,
            //    Arg3 = args.Arg4,
            //    Arg4 = args.Arg5,
            //    Arg5 = args.Arg6,
            //};

            serv.SwitchToThreadMethod(args);

            // will never get here, because service will call cmd_ExitServiceFunc, thats switching this this thread directly
            return 0;
        }

        private static uint cmd_ServiceReturn(SystemMessage* args)
        {
            var servThread = Scheduler.GetCurrentThread();
            var parent = servThread.ParentThread;

            servThread.ParentThread = null;
            parent.ChildThread = null;

            servThread.Status = ThreadStatus.Terminated;

            if (parent.StackState != null)
                parent.StackState->Stack.EAX = args->Arg2;

            Scheduler.SwitchToThread(parent.ThreadID);

            return 0;
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
            var args = new SystemMessage
            {
                Command = stack->EAX,
                Arg1 = stack->EBX,
                Arg2 = stack->ECX,
                Arg3 = stack->EDX,
                Arg4 = stack->ESI,
                Arg5 = stack->EDI,
                Arg6 = stack->EBP
            };

            const uint commandMask = BitsMask.Bits10;
            var commandNum = args.Command & commandMask;

            KernelMessage.WriteLine("Got SysCall cmd={0} arg1={1} arg2={2} arg3={3} arg4={4} arg5={5} arg6={6}", args.Command, args.Arg1, args.Arg2, args.Arg3, args.Arg4, args.Arg5, args.Arg6);

            Scheduler.SaveThreadState(Scheduler.GetCurrentThread().ThreadID, (IntPtr)stack);

            stack->EAX = Commands[commandNum].Handler(&args);
        }

        private static SysCallInfo[] Commands;
    }

    public unsafe delegate uint DSysCallInfoHandler(SystemMessage* args);

    public enum KnownSysCallCommand
    {
        RequestMemory = 20,
        ServiceReturn = 21,
        ServiceFunc1 = 22
    }

    public class SysCallInfo
    {
        public KnownSysCallCommand CommandID;
        //public uint Arguments;
        //public string Name;
        public DSysCallInfoHandler Handler;
    }

    //[StructLayout(LayoutKind.Sequential, Size = 4 * 5)]
    //public struct ServiceArgs
    //{
    //    public const uint Size = 4 * 5;

    //    public uint Arg1;
    //    public uint Arg2;
    //    public uint Arg3;
    //    public uint Arg4;
    //    public uint Arg5;
    //}

}
