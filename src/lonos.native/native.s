proc1:
mov bx, 0x3c3c         ; attrib/char of smiley
mov eax, 0x0b8000      ; note 32 bit offset
mov word [ds:eax], bx
ret
;jmp $

proc2:
mov bx, 0x3d3d         ; attrib/char of smiley
mov eax, 0x0b8002      ; note 32 bit offset
mov word [ds:eax], bx
ret
;jmp $

bochs_debug:
xchg ebx, ebx
;xchg bx, bx
ret