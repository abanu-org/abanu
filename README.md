[![Documentation Status](https://readthedocs.org/projects/lonos/badge/?version=latest)](http://docs.lonos.io/en/latest/?badge=latest) [![License][github-license]][github-license-link]  [![Issues][github-issues]][github-issues-link]  [![Stars][github-stars]][github-stars-link]  [![Forks][github-forks]][github-forks-link]

[github-forks]: https://img.shields.io/github/forks/lonos-project/lonos.svg
[github-forks-link]: https://github.com/lonos-project/lonos/network
[github-stars]: https://img.shields.io/github/stars/lonos-project/lonos.svg
[github-stars-link]: https://github.com/lonos-project/lonos/stargazers
[github-issues]: https://img.shields.io/github/issues/lonos-project/lonos.svg
[github-issues-link]: https://github.com/lonos-project/lonos/issues
[github-license]: https://img.shields.io/badge/license-GPL-blue.svg
[github-license-link]: https://raw.githubusercontent.com/lonos-project/lonos/master/LICENSE.txt
[![Join the chat at https://gitter.im/lonos-project/lonos](https://badges.gitter.im/lonos-project/lonos.svg)](https://gitter.im/lonos-project/lonos?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## Build instructions

#### Quick Start (recommended)

Visit [http://docs.lonos.io/en/latest/build.html](http://docs.lonos.io/en/latest/build.html)

#### Manually

Prerequisites:
- .NET Framework 4.7.2 or latest Mono
- NASM Assembler (`sudo apt-get install nasm`)
- mtools (`sudo apt-get install mtools`)
- xorriso (`sudo apt-get install xorriso`)
- Optional: qemu or bochs for emulation.

```
git clone --recursive https://github.com/lonos-project/lonos.git
cd lonos 
./lonosctl configure mosa        # Build the Mosa-Compiler
./lonosctl build all             # Builds the lonos kernel and creates a disk image
```
Run it with `./lonosctl run qemu x86` or `./lonosctl run bochs x86`

## The technology behind this project

- The most important part of the Lonos project is the [Mosa-Compiler](https://github.com/mosa/MOSA-Project), wich is written in pure C#. The Mosa-Compiler converts an already compiled Assembly (build via msbuild or xbuild, default compiler from .NET/Mono) into native Code.
- build some requied Assembler-Code  and append it to the native binary. The Assembler code ist mostly used for early initalization.
- Building the Operating System Disk Image, with Grub2 as Bootloader

## Status of the OS

This is a research project / proof of concept. So it isn't a fully functional OS. This is implemented:

- Build tool chain
- Reading Kernel-Embedded ELF-Files
- Integration of assembler code within the kernel.
- Output kernel log messages via serial interface to a text file on the host
- Scrollable screen output
- Setup GDT
- Basic Interrupts
- Basic Memory Protection
- Task-Switching
- User-Mode

## Contributing

Feel free to contact Arakis, open a Issue or a Pull Request.

## License
Lonos is published under the GNU General Public License (Version 2, only). This software includes third party open source software components. Each of these software components have their own license. All sourcecode is open source.
