[BITS 32]
push ebp
mov ebp, esp

pushfd
pop eax ; TODO: Optimize: Load EAX from ESP
push eax
popfd

pop ebp
ret
