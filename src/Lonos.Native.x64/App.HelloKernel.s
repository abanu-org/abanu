[BITS 32]
push ebp
mov ebp, esp
mov eax,[ebp+0x8+4*0] ;command
mov ebx,[ebp+0x8+4*1] ;arg1
mov ecx,[ebp+0x8+4*2] ;arg2
mov edx,[ebp+0x8+4*3] ;arg3
mov esi,[ebp+0x8+4*4] ;arg4
mov edi,[ebp+0x8+4*5] ;arg5
mov ebp,[ebp+0x8+4*6] ;arg6
int 250
; result is in eax
pop ebp
ret
