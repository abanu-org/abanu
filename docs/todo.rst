TODO
====

Compiler:
  | - STACK_TOP configurable
  | - Patch for ProgramHeader [DONE]
  | - Insert Comment for explaination why needing Running InitializeAssembly() manually [DONE]
  | - Adding more DWARF sections to debug local variables.

Kernel:
  | - Check Permission of all SysCalls, like GetPhysicalMemroy
  | - Don't run all apps with full IOCPL permissions
  | - Add interprocess syncronization mechanisms like mutex and semaphore.
  | - no kernel panic if user apps is crashing
  | - Switch vom Text Mode to Graphics Mode without boot loader, if booted in text mode.
  | - Add x64 support

Services:
  | - avoid using "unsafe" keyword.
  | - Using more ref structs.
  | - Finalize File-System Interface
  | - Free Handles on User app quit
  | - Ext2-Driver
  | - Network stack
  | - Basic USB-Support
  | - Move ConsoleHost to central Service
  | - Multiplexing ConsoleHost for multiple clients / allow multiple virtual consoles

Libs:
  | Implement Memory-/Allocation-friendly Dictionary "KDictionary".
  | Porting "newlib" with basic SysCalls.

Apps:
  | Simple Command Line Interpreter
  | Porting true, false, cat, readline and echo, as native c apps for proofe of concept unix app tests, based on newlib.

Tools:
  | Adding automated tests
  | Optimize Argument parsing of lonosctl to allow optional parameters (--name value) and bulk actions (build a,b,c)
  | Attaching "Networkdisk" to all kinds of qemu runs.
  | Att helper function to lonosctl to start/stop HostCommunication.exe