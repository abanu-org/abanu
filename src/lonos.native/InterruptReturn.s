[BITS 32]
push ebp
mov ebp, esp
mov esi, [ebp+0x8]	;get stack pointer argument
mov esp, esi		;set stack pointer

mov eax, dword 0x10
mov gs,eax
mov ds,eax
mov fs,eax
mov es,eax

;mov [esp+13*4],esi
;mov [esp+14*4],dword 0x20
popad
add esp, 0x8

;mov [esp], dword 0xBBAABBAA
;mov [esp+4], dword 0xCCDDCCDD

sti
iretd
