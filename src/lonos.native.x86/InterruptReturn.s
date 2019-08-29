[BITS 32]
push ebp
mov ebp, esp
mov esi, [ebp+8]	;get stack pointer argument
mov esp, esi		;set stack pointer

mov eax, [ebp+12]   ;get selector
mov gs,eax
mov ds,eax
mov fs,eax
mov es,eax

popad
add esp, 0x8

iretd
