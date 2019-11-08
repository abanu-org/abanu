[BITS 32]
push ebp
mov ebp, esp

mov esi,[ebp+8]
mov eax,[ebp+12]
mov [fs:esi],eax

pop ebp
ret
