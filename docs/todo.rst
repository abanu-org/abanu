TODO
====

Compiler:

  | - STACK_TOP configurable
  | - Patch for ProgramHeader [DONE]
  | - Insert Comment for explaination why needing Running InitializeAssembly() manually [DONE]

Kernel:
  | - Check Permission of all SysCalls, like GetPhysicalMemroy
  | - Don't run all apps with full IOCPL permissions
  | - Add interprocess syncronization mechanisms like mutex and semaphore.
  | - no kernel panic if user apps is crashing
  | - Switch vom Text Mode to Graphics Mode without boot loader, if booted in text mode.
  | - Add x64 support

