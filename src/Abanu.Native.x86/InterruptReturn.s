[BITS 32]
push ebp
mov ebp, esp
mov esi, [ebp+8]	;get stack pointer argument
mov esp, esi		;set stack pointer

mov eax, [ebp+16]   ;get selector

mov ds,eax
mov gs,eax
mov es,eax

mov eax, [ebp+20]   ;get gs selector
mov fs,eax

mov eax, [ebp+12]   ;get cr3 argument
mov [esp+8*4], eax  ;store cr3 in stack

popad

push eax
mov eax, [esp+4] ;cr3 value
mov cr3, eax
pop eax

add esp, 0x8

iretd
