[BITS 32]
push ebp
mov ebp, esp

mov ax, [ebp+0x8]
ltr ax

pop ebp
ret
