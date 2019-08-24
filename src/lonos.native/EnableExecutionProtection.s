[BITS 32]
mov ecx, 0xc0000080
rdmsr
or eax, 0x800
wrmsr
ret
