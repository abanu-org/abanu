[BITS 32]
push ebp
mov ebp, esp
cli

mov eax, [ebp+0x8]	;get stack pointer argument

push dword 0x23       ; push user ss
push eax       ; push user esp

pushf

pop eax            ;
or eax, dword 0x200      ;
and eax, dword 0xffffbfff   ;
push eax         ; re-enable interupt after switch

push dword 0x1B       ; push user cs (0x23)
push dword 0x22222222       ; push code offset

mov ax, 0x23   ; user data segement
mov ds, ax
mov es, ax
mov fs, ax
mov gs, ax

iretd
