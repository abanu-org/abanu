test_proc1:
mov bx, 0x3c3c         ; attrib/char of smiley
mov eax, 0x0b8000      ; note 32 bit offset
mov word [ds:eax], bx
ret

test_proc2:
mov bx, 0x3d3d         ; attrib/char of smiley
mov eax, 0x0b8002      ; note 32 bit offset
mov word [ds:eax], bx
ret

bochs_debug:
xchg bx, bx
ret
