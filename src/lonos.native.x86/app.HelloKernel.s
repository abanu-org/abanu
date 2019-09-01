[BITS 32]
push ebp
mov ebp, esp
mov eax,[ebp+0x8] ;command
mov ebx,[ebp+0x12] ;arg0
int 250
; result is in eax
pop ebp
ret
