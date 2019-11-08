[BITS 32]
push ebp
mov ebp, esp

mov esi,[ebp+0x8]
mov eax,[fs:esi]

pop ebp
ret
