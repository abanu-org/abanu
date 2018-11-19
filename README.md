## Build instructions

Prerequisites:
- .NET Framework 4.6.1 or latest Mono
- NASM Assembler
- Optional: qemu or bochs for emulation.

```
git clone --recursive https://github.com/Arakis/lonos.git
cd lonos 
./lonosctl configure patch apply # Some patches are requied, because Mono does not support .NET Framework v4.7.2 yet
./lonosctl configure mosa        # Build the Mosa-Compiler
./lonosctl build all             # Builds the lonos kernel and creates a disk image
```
Run it with `./lonosctl run qemu x86` or `./lonosctl run bochs x86`

## The technology behind this

- The most important part of the Lonos project is the [Mosa-Compiler](https://github.com/mosa/MOSA-Project), wich is written in pure C#. The Mosa-Compiler converts an already compiled Assembly (build via msbuild or xbuild, default compiler from .NET/Mono) into native Code.
- build some requied Assembler-Code  and append it to the native binary. The Assembler code ist mostly used for early initalization.
- Building the Operating System Disk Image, with Grub2 as Bootloader

## The technology behind this

This is a research project / proof of concept. So it isn't a fully functional OS. This is implemented:

- Build tool chain
- Reading Kernel-Embedded ELF-Files
- Integration of assembler code within the kernel.
- Output kernel log messages via serial interface to a text file on the host
- Scrollable screen output
- Setup GDT
- Basic Interrupts

## Contributing

Feel free to contact Arakis, open a Issue or a Pull Request.

## License
Lonos is published under the GNU General Public License. This software includes third party open source software components. Each of these software components have their own license. All sourcecode is open source.
