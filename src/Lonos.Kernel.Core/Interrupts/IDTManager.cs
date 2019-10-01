// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Lonos.Kernel.Core.MemoryManagement;
using Lonos.Kernel.Core.PageManagement;
using Lonos.Kernel.Core.Scheduling;
using Lonos.Kernel.Core.SysCalls;
using Mosa.Runtime;
using Mosa.Runtime.x86;

namespace Lonos.Kernel.Core.Interrupts
{
    public unsafe delegate void InterruptHandler(IDTStack* stack);

    /// <summary>
    /// IDT
    /// </summary>
    public static unsafe class IDTManager
    {

        #region Data Members

        internal struct Offset
        {
            internal const byte BaseLow = 0x00;
            internal const byte Select = 0x02;
            internal const byte Always0 = 0x04;
            internal const byte Flags = 0x05;
            internal const byte BaseHigh = 0x06;
            internal const byte TotalSize = 0x08;
        }

        #endregion Data Members

        internal static InterruptInfo[] Handlers;

        private static Addr IDTAddr;
        public static InterruptControlBlock* ControlBlock;

        public static void Setup()
        {
            KernelMessage.WriteLine("Setup IDT");

            InitControlBlock();

            IDTAddr = PhysicalPageManager.AllocatePage()->Address;
            PageTable.KernelTable.SetWritable(IDTAddr, 4096);
            KernelMessage.WriteLine("Address of IDT: {0:X8}", IDTAddr);

            // Setup IDT table
            Mosa.Runtime.Internal.MemoryClear(new Pointer((uint)IDTAddr), 6);
            Intrinsic.Store16(new Pointer((uint)IDTAddr), (Offset.TotalSize * 256) - 1);
            Intrinsic.Store32(new Pointer((uint)IDTAddr), 2, IDTAddr + 6);

            KernelMessage.Write("Set IDT table entries...");
            SetTableEntries();
            KernelMessage.WriteLine("done");

            Handlers = new InterruptInfo[256];
            for (var i = 0; i <= 255; i++)
            {
                var info = new InterruptInfo
                {
                    Interrupt = i,
                    CountStatistcs = true,
                    Trace = true,
                    Handler = InterruptHandlers.Undefined,
                };
                if (i == (int)KnownInterrupt.ClockTimer)
                {
                    info.Trace = false;
                    info.CountStatistcs = false;
                }
                Handlers[i] = info;
            }

            SetInterruptHandler(KnownInterrupt.DivideError, InterruptHandlers.DivideError);
            SetInterruptHandler(KnownInterrupt.ArithmeticOverflowException, InterruptHandlers.ArithmeticOverflowException);
            SetInterruptHandler(KnownInterrupt.BoundCheckError, InterruptHandlers.BoundCheckError);
            SetInterruptHandler(KnownInterrupt.InvalidOpcode, InterruptHandlers.InvalidOpcode);
            SetInterruptHandler(KnownInterrupt.CoProcessorNotAvailable, InterruptHandlers.CoProcessorNotAvailable);
            SetInterruptHandler(KnownInterrupt.DoubleFault, InterruptHandlers.DoubleFault);
            SetInterruptHandler(KnownInterrupt.CoProcessorSegmentOverrun, InterruptHandlers.CoProcessorSegmentOverrun);
            SetInterruptHandler(KnownInterrupt.InvalidTSS, InterruptHandlers.InvalidTSS);
            SetInterruptHandler(KnownInterrupt.SegmentNotPresent, InterruptHandlers.SegmentNotPresent);
            SetInterruptHandler(KnownInterrupt.StackException, InterruptHandlers.StackException);
            SetInterruptHandler(KnownInterrupt.GeneralProtectionException, InterruptHandlers.GeneralProtectionException);
            SetInterruptHandler(KnownInterrupt.PageFault, InterruptHandlers.PageFault);
            SetInterruptHandler(KnownInterrupt.CoProcessorError, InterruptHandlers.CoProcessorError);
            SetInterruptHandler(KnownInterrupt.SIMDFloatinPointException, InterruptHandlers.SIMDFloatinPointException);
            SetInterruptHandler(KnownInterrupt.ClockTimer, InterruptHandlers.ClockTimer);
            SetInterruptHandler(KnownInterrupt.Keyboard, InterruptHandlers.Keyboard);
            SetInterruptHandler(KnownInterrupt.TerminateCurrentThread, InterruptHandlers.TermindateCurrentThread);

            KernelMessage.Write("Enabling interrupts...");

            Flush();

            IDT.Enabled = true
            ;
            KernelMessage.WriteLine("done");
        }

        private static void InitControlBlock()
        {
            var p = PhysicalPageManager.AllocatePage()->Address;
            PageTable.KernelTable.Map(Address.InterruptControlBlock, p, 4096, flush: true);
            PageTable.KernelTable.SetWritable(Address.InterruptControlBlock, 4096);
            ControlBlock = (InterruptControlBlock*)Address.InterruptControlBlock;
            ControlBlock->KernelPageTableAddr = PageTable.KernelTable.GetPageTablePhysAddr();
        }

        public static void Flush()
        {
            var idtAddr = (uint)IDTAddr;
            Native.Lidt(idtAddr);
            Native.Sti();
        }

        internal static void SetInterruptHandler(KnownInterrupt interrupt, InterruptHandler interruptHandler)
        {
            SetInterruptHandler((uint)interrupt, interruptHandler);
        }

        internal static void SetInterruptHandler(uint interrupt, InterruptHandler interruptHandler, Service service = null)
        {
            if (interruptHandler == null)
                interruptHandler = InterruptHandlers.Undefined;
            Handlers[interrupt].Handler = interruptHandler;
            Handlers[interrupt].Service = service;
        }

        #region SetTable Entries

        /// <summary>
        /// Sets the IDT.
        /// </summary>
        private static void SetTableEntries()
        {
            // Clear out IDT table
            Mosa.Runtime.Internal.MemoryClear(new Pointer((void*)IDTAddr) + 6, Offset.TotalSize * 256);

            Set(0, IRQ0);
            Set(1, IRQ1);
            Set(2, IRQ2);
            Set(3, IRQ3);
            Set(4, IRQ4);
            Set(5, IRQ5);
            Set(6, IRQ6);
            Set(7, IRQ7);
            Set(8, IRQ8);
            Set(9, IRQ9);
            Set(10, IRQ10);
            Set(11, IRQ11);
            Set(12, IRQ12);
            Set(13, IRQ13);
            Set(14, IRQ14);
            Set(15, IRQ15);
            Set(16, IRQ16);
            Set(17, IRQ17);
            Set(18, IRQ18);
            Set(19, IRQ19);
            Set(20, IRQ20);
            Set(21, IRQ21);
            Set(22, IRQ22);
            Set(23, IRQ23);
            Set(24, IRQ24);
            Set(25, IRQ25);
            Set(26, IRQ26);
            Set(27, IRQ27);
            Set(28, IRQ28);
            Set(29, IRQ29);
            Set(30, IRQ30);
            Set(31, IRQ31);
            Set(32, IRQ32);
            Set(33, IRQ33);
            Set(34, IRQ34);
            Set(35, IRQ35);
            Set(36, IRQ36);
            Set(37, IRQ37);
            Set(38, IRQ38);
            Set(39, IRQ39);
            Set(40, IRQ40);
            Set(41, IRQ41);
            Set(42, IRQ42);
            Set(43, IRQ43);
            Set(44, IRQ44);
            Set(45, IRQ45);
            Set(46, IRQ46);
            Set(47, IRQ47);
            Set(48, IRQ48);
            Set(49, IRQ49);
            Set(50, IRQ50);
            Set(51, IRQ51);
            Set(52, IRQ52);
            Set(53, IRQ53);
            Set(54, IRQ54);
            Set(55, IRQ55);
            Set(56, IRQ56);
            Set(57, IRQ57);
            Set(58, IRQ58);
            Set(59, IRQ59);
            Set(60, IRQ60);
            Set(61, IRQ61);
            Set(62, IRQ62);
            Set(63, IRQ63);
            Set(64, IRQ64);
            Set(65, IRQ65);
            Set(66, IRQ66);
            Set(67, IRQ67);
            Set(68, IRQ68);
            Set(69, IRQ69);
            Set(70, IRQ70);
            Set(71, IRQ71);
            Set(72, IRQ72);
            Set(73, IRQ73);
            Set(74, IRQ74);
            Set(75, IRQ75);
            Set(76, IRQ76);
            Set(77, IRQ77);
            Set(78, IRQ78);
            Set(79, IRQ79);
            Set(80, IRQ80);
            Set(81, IRQ81);
            Set(82, IRQ82);
            Set(83, IRQ83);
            Set(84, IRQ84);
            Set(85, IRQ85);
            Set(86, IRQ86);
            Set(87, IRQ87);
            Set(88, IRQ88);
            Set(89, IRQ89);
            Set(90, IRQ90);
            Set(91, IRQ91);
            Set(92, IRQ92);
            Set(93, IRQ93);
            Set(94, IRQ94);
            Set(95, IRQ95);
            Set(96, IRQ96);
            Set(97, IRQ97);
            Set(98, IRQ98);
            Set(99, IRQ99);
            Set(100, IRQ100);
            Set(101, IRQ101);
            Set(102, IRQ102);
            Set(103, IRQ103);
            Set(104, IRQ104);
            Set(105, IRQ105);
            Set(106, IRQ106);
            Set(107, IRQ107);
            Set(108, IRQ108);
            Set(109, IRQ109);
            Set(110, IRQ110);
            Set(111, IRQ111);
            Set(112, IRQ112);
            Set(113, IRQ113);
            Set(114, IRQ114);
            Set(115, IRQ115);
            Set(116, IRQ116);
            Set(117, IRQ117);
            Set(118, IRQ118);
            Set(119, IRQ119);
            Set(120, IRQ120);
            Set(121, IRQ121);
            Set(122, IRQ122);
            Set(123, IRQ123);
            Set(124, IRQ124);
            Set(125, IRQ125);
            Set(126, IRQ126);
            Set(127, IRQ127);
            Set(128, IRQ128);
            Set(129, IRQ129);
            Set(130, IRQ130);
            Set(131, IRQ131);
            Set(132, IRQ132);
            Set(133, IRQ133);
            Set(134, IRQ134);
            Set(135, IRQ135);
            Set(136, IRQ136);
            Set(137, IRQ137);
            Set(138, IRQ138);
            Set(139, IRQ139);
            Set(140, IRQ140);
            Set(141, IRQ141);
            Set(142, IRQ142);
            Set(143, IRQ143);
            Set(144, IRQ144);
            Set(145, IRQ145);
            Set(146, IRQ146);
            Set(147, IRQ147);
            Set(148, IRQ148);
            Set(149, IRQ149);
            Set(150, IRQ150);
            Set(151, IRQ151);
            Set(152, IRQ152);
            Set(153, IRQ153);
            Set(154, IRQ154);
            Set(155, IRQ155);
            Set(156, IRQ156);
            Set(157, IRQ157);
            Set(158, IRQ158);
            Set(159, IRQ159);
            Set(160, IRQ160);
            Set(161, IRQ161);
            Set(162, IRQ162);
            Set(163, IRQ163);
            Set(164, IRQ164);
            Set(165, IRQ165);
            Set(166, IRQ166);
            Set(167, IRQ167);
            Set(168, IRQ168);
            Set(169, IRQ169);
            Set(170, IRQ170);
            Set(171, IRQ171);
            Set(172, IRQ172);
            Set(173, IRQ173);
            Set(174, IRQ174);
            Set(175, IRQ175);
            Set(176, IRQ176);
            Set(177, IRQ177);
            Set(178, IRQ178);
            Set(179, IRQ179);
            Set(180, IRQ180);
            Set(181, IRQ181);
            Set(182, IRQ182);
            Set(183, IRQ183);
            Set(184, IRQ184);
            Set(185, IRQ185);
            Set(186, IRQ186);
            Set(187, IRQ187);
            Set(188, IRQ188);
            Set(189, IRQ189);
            Set(190, IRQ190);
            Set(191, IRQ191);
            Set(192, IRQ192);
            Set(193, IRQ193);
            Set(194, IRQ194);
            Set(195, IRQ195);
            Set(196, IRQ196);
            Set(197, IRQ197);
            Set(198, IRQ198);
            Set(199, IRQ199);
            Set(200, IRQ200);
            Set(201, IRQ201);
            Set(202, IRQ202);
            Set(203, IRQ203);
            Set(204, IRQ204);
            Set(205, IRQ205);
            Set(206, IRQ206);
            Set(207, IRQ207);
            Set(208, IRQ208);
            Set(209, IRQ209);
            Set(210, IRQ210);
            Set(211, IRQ211);
            Set(212, IRQ212);
            Set(213, IRQ213);
            Set(214, IRQ214);
            Set(215, IRQ215);
            Set(216, IRQ216);
            Set(217, IRQ217);
            Set(218, IRQ218);
            Set(219, IRQ219);
            Set(220, IRQ220);
            Set(221, IRQ221);
            Set(222, IRQ222);
            Set(223, IRQ223);
            Set(224, IRQ224);
            Set(225, IRQ225);
            Set(226, IRQ226);
            Set(227, IRQ227);
            Set(228, IRQ228);
            Set(229, IRQ229);
            Set(230, IRQ230);
            Set(231, IRQ231);
            Set(232, IRQ232);
            Set(233, IRQ233);
            Set(234, IRQ234);
            Set(235, IRQ235);
            Set(236, IRQ236);
            Set(237, IRQ237);
            Set(238, IRQ238);
            Set(239, IRQ239);
            Set(240, IRQ240);
            Set(241, IRQ241);
            Set(242, IRQ242);
            Set(243, IRQ243);
            Set(244, IRQ244);
            Set(245, IRQ245);
            Set(246, IRQ246);
            Set(247, IRQ247);
            Set(248, IRQ248);
            Set(249, IRQ249);
            Set(250, IRQ250);
            Set(251, IRQ251);
            Set(252, IRQ252);
            Set(253, IRQ253);
            Set(254, IRQ254);
            Set(255, IRQ255);
        }

        #endregion

        #region IRQ Implementation Methods

        private static void IRQ0()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ0();
        }

        private static void IRQ1()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ1();
        }

        private static void IRQ2()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ2();
        }

        private static void IRQ3()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ3();
        }

        private static void IRQ4()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ4();
        }

        private static void IRQ5()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ5();
        }

        private static void IRQ6()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ6();
        }

        private static void IRQ7()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ7();
        }

        private static void IRQ8()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ8();
        }

        private static void IRQ9()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ9();
        }

        // Invalid TSS
        private static void IRQ10()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ10();
        }

        private static void IRQ11()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ11();
        }

        private static void IRQ12()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ12();
        }

        // General Protection Exception
        private static void IRQ13()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ13();
        }

        // Page Fault
        private static void IRQ14()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ14();
        }

        private static void IRQ15()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ15();
        }

        private static void IRQ16()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ16();
        }

        private static void IRQ17()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ17();
        }

        private static void IRQ18()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ18();
        }

        private static void IRQ19()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ19();
        }

        private static void IRQ20()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ20();
        }

        private static void IRQ21()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ21();
        }

        private static void IRQ22()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ22();
        }

        private static void IRQ23()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ23();
        }

        private static void IRQ24()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ24();
        }

        private static void IRQ25()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ25();
        }

        private static void IRQ26()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ26();
        }

        private static void IRQ27()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ27();
        }

        private static void IRQ28()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ28();
        }

        private static void IRQ29()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ29();
        }

        private static void IRQ30()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ30();
        }

        private static void IRQ31()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ31();
        }

        private static void IRQ32()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ32();
        }

        private static void IRQ33()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ33();
        }

        private static void IRQ34()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ34();
        }

        private static void IRQ35()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ35();
        }

        private static void IRQ36()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ36();
        }

        private static void IRQ37()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ37();
        }

        private static void IRQ38()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ38();
        }

        private static void IRQ39()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ39();
        }

        private static void IRQ40()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ40();
        }

        private static void IRQ41()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ41();
        }

        private static void IRQ42()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ42();
        }

        private static void IRQ43()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ43();
        }

        private static void IRQ44()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ44();
        }

        private static void IRQ45()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ45();
        }

        private static void IRQ46()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ46();
        }

        private static void IRQ47()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ47();
        }

        private static void IRQ48()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ48();
        }

        private static void IRQ49()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ49();
        }

        private static void IRQ50()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ50();
        }

        private static void IRQ51()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ51();
        }

        private static void IRQ52()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ52();
        }

        private static void IRQ53()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ53();
        }

        private static void IRQ54()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ54();
        }

        private static void IRQ55()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ55();
        }

        private static void IRQ56()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ56();
        }

        private static void IRQ57()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ57();
        }

        private static void IRQ58()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ58();
        }

        private static void IRQ59()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ59();
        }

        private static void IRQ60()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ60();
        }

        private static void IRQ61()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ61();
        }

        private static void IRQ62()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ62();
        }

        private static void IRQ63()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ63();
        }

        private static void IRQ64()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ64();
        }

        private static void IRQ65()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ65();
        }

        private static void IRQ66()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ66();
        }

        private static void IRQ67()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ67();
        }

        private static void IRQ68()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ68();
        }

        private static void IRQ69()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ69();
        }

        private static void IRQ70()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ70();
        }

        private static void IRQ71()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ71();
        }

        private static void IRQ72()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ72();
        }

        private static void IRQ73()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ73();
        }

        private static void IRQ74()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ74();
        }

        private static void IRQ75()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ75();
        }

        private static void IRQ76()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ76();
        }

        private static void IRQ77()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ77();
        }

        private static void IRQ78()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ78();
        }

        private static void IRQ79()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ79();
        }

        private static void IRQ80()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ80();
        }

        private static void IRQ81()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ81();
        }

        private static void IRQ82()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ82();
        }

        private static void IRQ83()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ83();
        }

        private static void IRQ84()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ84();
        }

        private static void IRQ85()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ85();
        }

        private static void IRQ86()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ86();
        }

        private static void IRQ87()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ87();
        }

        private static void IRQ88()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ88();
        }

        private static void IRQ89()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ89();
        }

        private static void IRQ90()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ90();
        }

        private static void IRQ91()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ91();
        }

        private static void IRQ92()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ92();
        }

        private static void IRQ93()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ93();
        }

        private static void IRQ94()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ94();
        }

        private static void IRQ95()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ95();
        }

        private static void IRQ96()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ96();
        }

        private static void IRQ97()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ97();
        }

        private static void IRQ98()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ98();
        }

        private static void IRQ99()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ99();
        }

        private static void IRQ100()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ100();
        }

        private static void IRQ101()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ101();
        }

        private static void IRQ102()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ102();
        }

        private static void IRQ103()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ103();
        }

        private static void IRQ104()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ104();
        }

        private static void IRQ105()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ105();
        }

        private static void IRQ106()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ106();
        }

        private static void IRQ107()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ107();
        }

        private static void IRQ108()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ108();
        }

        private static void IRQ109()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ109();
        }

        private static void IRQ110()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ110();
        }

        private static void IRQ111()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ111();
        }

        private static void IRQ112()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ112();
        }

        private static void IRQ113()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ113();
        }

        private static void IRQ114()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ114();
        }

        private static void IRQ115()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ115();
        }

        private static void IRQ116()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ116();
        }

        private static void IRQ117()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ117();
        }

        private static void IRQ118()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ118();
        }

        private static void IRQ119()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ119();
        }

        private static void IRQ120()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ120();
        }

        private static void IRQ121()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ121();
        }

        private static void IRQ122()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ122();
        }

        private static void IRQ123()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ123();
        }

        private static void IRQ124()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ124();
        }

        private static void IRQ125()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ125();
        }

        private static void IRQ126()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ126();
        }

        private static void IRQ127()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ127();
        }

        private static void IRQ128()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ128();
        }

        private static void IRQ129()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ129();
        }

        private static void IRQ130()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ130();
        }

        private static void IRQ131()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ131();
        }

        private static void IRQ132()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ132();
        }

        private static void IRQ133()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ133();
        }

        private static void IRQ134()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ134();
        }

        private static void IRQ135()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ135();
        }

        private static void IRQ136()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ136();
        }

        private static void IRQ137()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ137();
        }

        private static void IRQ138()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ138();
        }

        private static void IRQ139()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ139();
        }

        private static void IRQ140()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ140();
        }

        private static void IRQ141()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ141();
        }

        private static void IRQ142()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ142();
        }

        private static void IRQ143()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ143();
        }

        private static void IRQ144()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ144();
        }

        private static void IRQ145()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ145();
        }

        private static void IRQ146()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ146();
        }

        private static void IRQ147()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ147();
        }

        private static void IRQ148()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ148();
        }

        private static void IRQ149()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ149();
        }

        private static void IRQ150()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ150();
        }

        private static void IRQ151()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ151();
        }

        private static void IRQ152()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ152();
        }

        private static void IRQ153()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ153();
        }

        private static void IRQ154()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ154();
        }

        private static void IRQ155()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ155();
        }

        private static void IRQ156()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ156();
        }

        private static void IRQ157()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ157();
        }

        private static void IRQ158()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ158();
        }

        private static void IRQ159()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ159();
        }

        private static void IRQ160()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ160();
        }

        private static void IRQ161()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ161();
        }

        private static void IRQ162()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ162();
        }

        private static void IRQ163()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ163();
        }

        private static void IRQ164()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ164();
        }

        private static void IRQ165()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ165();
        }

        private static void IRQ166()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ166();
        }

        private static void IRQ167()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ167();
        }

        private static void IRQ168()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ168();
        }

        private static void IRQ169()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ169();
        }

        private static void IRQ170()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ170();
        }

        private static void IRQ171()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ171();
        }

        private static void IRQ172()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ172();
        }

        private static void IRQ173()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ173();
        }

        private static void IRQ174()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ174();
        }

        private static void IRQ175()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ175();
        }

        private static void IRQ176()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ176();
        }

        private static void IRQ177()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ177();
        }

        private static void IRQ178()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ178();
        }

        private static void IRQ179()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ179();
        }

        private static void IRQ180()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ180();
        }

        private static void IRQ181()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ181();
        }

        private static void IRQ182()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ182();
        }

        private static void IRQ183()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ183();
        }

        private static void IRQ184()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ184();
        }

        private static void IRQ185()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ185();
        }

        private static void IRQ186()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ186();
        }

        private static void IRQ187()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ187();
        }

        private static void IRQ188()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ188();
        }

        private static void IRQ189()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ189();
        }

        private static void IRQ190()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ190();
        }

        private static void IRQ191()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ191();
        }

        private static void IRQ192()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ192();
        }

        private static void IRQ193()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ193();
        }

        private static void IRQ194()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ194();
        }

        private static void IRQ195()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ195();
        }

        private static void IRQ196()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ196();
        }

        private static void IRQ197()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ197();
        }

        private static void IRQ198()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ198();
        }

        private static void IRQ199()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ199();
        }

        private static void IRQ200()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ200();
        }

        private static void IRQ201()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ201();
        }

        private static void IRQ202()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ202();
        }

        private static void IRQ203()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ203();
        }

        private static void IRQ204()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ204();
        }

        private static void IRQ205()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ205();
        }

        private static void IRQ206()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ206();
        }

        private static void IRQ207()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ207();
        }

        private static void IRQ208()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ208();
        }

        private static void IRQ209()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ209();
        }

        private static void IRQ210()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ210();
        }

        private static void IRQ211()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ211();
        }

        private static void IRQ212()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ212();
        }

        private static void IRQ213()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ213();
        }

        private static void IRQ214()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ214();
        }

        private static void IRQ215()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ215();
        }

        private static void IRQ216()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ216();
        }

        private static void IRQ217()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ217();
        }

        private static void IRQ218()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ218();
        }

        private static void IRQ219()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ219();
        }

        private static void IRQ220()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ220();
        }

        private static void IRQ221()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ221();
        }

        private static void IRQ222()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ222();
        }

        private static void IRQ223()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ223();
        }

        private static void IRQ224()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ224();
        }

        private static void IRQ225()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ225();
        }

        private static void IRQ226()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ226();
        }

        private static void IRQ227()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ227();
        }

        private static void IRQ228()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ228();
        }

        private static void IRQ229()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ229();
        }

        private static void IRQ230()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ230();
        }

        private static void IRQ231()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ231();
        }

        private static void IRQ232()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ232();
        }

        private static void IRQ233()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ233();
        }

        private static void IRQ234()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ234();
        }

        private static void IRQ235()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ235();
        }

        private static void IRQ236()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ236();
        }

        private static void IRQ237()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ237();
        }

        private static void IRQ238()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ238();
        }

        private static void IRQ239()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ239();
        }

        private static void IRQ240()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ240();
        }

        private static void IRQ241()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ241();
        }

        private static void IRQ242()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ242();
        }

        private static void IRQ243()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ243();
        }

        private static void IRQ244()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ244();
        }

        private static void IRQ245()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ245();
        }

        private static void IRQ246()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ246();
        }

        private static void IRQ247()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ247();
        }

        private static void IRQ248()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ248();
        }

        private static void IRQ249()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ249();
        }

        private static void IRQ250()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ250();
        }

        private static void IRQ251()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ251();
        }

        private static void IRQ252()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ252();
        }

        private static void IRQ253()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ253();
        }

        private static void IRQ254()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ254();
        }

        private static void IRQ255()
        {
            Intrinsic.SuppressStackFrame();
            Native.IRQ255();
        }

        #endregion IRQ Implementation Methods

        private static void Set(uint irq, Action interruptEntryMethod)
        {
            //KernelMessage.WriteLine("set {0}", index);
            var p = Marshal.GetFunctionPointerForDelegate(interruptEntryMethod);
            //KernelMessage.WriteLine("got p: {0:X8}", (Addr)p);

            Set(irq, (uint)p.ToInt32(), 0x08, 0x8E);
        }

        /// <summary>
        /// Sets the specified index.
        /// </summary>
        /// <param name="irq">The index.</param>
        /// <param name="address">The address.</param>
        /// <param name="select">The select.</param>
        /// <param name="flags">The flags.</param>
        private static void Set(uint irq, uint address, ushort select, byte flags)
        {
            var entry = new Pointer(IDTAddr + 6 + (irq * Offset.TotalSize));
            Intrinsic.Store16(entry, Offset.BaseLow, (ushort)(address & 0xFFFF));
            Intrinsic.Store16(entry, Offset.BaseHigh, (ushort)(address >> 16 & 0xFFFF));
            Intrinsic.Store16(entry, Offset.Select, select);
            Intrinsic.Store8(entry, Offset.Always0, 0);
            Intrinsic.Store8(entry, Offset.Flags, flags);
        }

        public static void SetPrivilegeLevel(uint irq, byte privLevel)
        {
            var entry = new Pointer(IDTAddr + 6 + (irq * Offset.TotalSize));
            var value = Intrinsic.Load8(entry, Offset.Flags);
            value = value.SetBits(5, 2, privLevel);
            Intrinsic.Store8(entry, Offset.Flags, value);
        }

        internal static uint RaisedCount;
        internal static uint RaisedCountCustom;

    }
}
