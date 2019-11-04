[![License][github-license]][github-license-link]  [![Issues][github-issues]][github-issues-link]  [![Stars][github-stars]][github-stars-link]  [![Forks][github-forks]][github-forks-link]

[github-forks]: https://img.shields.io/github/forks/abanu-org/abanu.svg
[github-forks-link]: https://github.com/abanu-org/abanu/network
[github-stars]: https://img.shields.io/github/stars/abanu-org/abanu.svg
[github-stars-link]: https://github.com/abanu-org/abanu/stargazers
[github-issues]: https://img.shields.io/github/issues/abanu-org/abanu.svg
[github-issues-link]: https://github.com/abanu-org/abanu/issues
[github-license]: https://img.shields.io/badge/license-MIT-blue.svg
[github-license-link]: https://raw.githubusercontent.com/abanu-org/abanu/master/LICENSE.txt
[![Join the chat at https://gitter.im/abanu-org/abanu](https://badges.gitter.im/abanu-org/abanu.svg)](https://gitter.im/abanu-org/abanu?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Join Discord https://discord.gg/dZVMBnE](https://img.shields.io/discord/638041194408181770?logo=discord)](https://discord.gg/dZVMBnE)

[![Actions Status](https://github.com/abanu-org/abanu/workflows/Tests/badge.svg)](https://github.com/abanu-org/abanu/actions) [![Documentation Status](https://readthedocs.org/projects/abanu/badge/?version=latest)](http://docs.abanu.org/en/latest/?badge=latest)

## About Abanu

Abanu is an Operating System written in C#.

## Getting the Sources

Visit the official repository [https://github.com/abanu-org/abanu](https://github.com/abanu-org/abanu)

## Build instructions

Visit: [https://docs.abanu.org/en/latest/build.html](http://docs.abanu.org/en/latest/build.html)

## The technology behind this project

- The most important part of the Abanu project is the [Mosa-Compiler](https://github.com/mosa/MOSA-Project), which is written in pure C#. The Mosa-Compiler converts an already compiled Assembly (build via msbuild or xbuild, default compiler from .NET/Mono) into native Code.
- build some required Assembler-Code  and append it to the native binary. The Assembler code is mostly used for early initialization.
- Building the Operating System Disk Image, with Grub2 as Bootloader

## Status of the OS

This project is in active development. This is already implemented:

- Build tool chain
- Automated Integration Tests
- Reading Kernel-Embedded ELF-Files
- Integration of assembler code within the kernel.
- Output kernel log messages via serial interface to a text file on the host
- Scrollable screen output
- Basic Memory Protection
- Task-Switching
- User-Mode
- Console-Server
- Basic Display-Serer
- Graphical Demo
- Integrated [Mosa Drivers](https://github.com/mosa/MOSA-Project/tree/master/Source/Mosa.DeviceDriver)
- Basic File Operations
- Basic Interprocess Communication

## Contributing

Feel free to contact us, open a Issue or a Pull Request.

## License
Abanu is published under the MIT License. This software includes third party open source software components. Each of these software components have their own license. All sourcecode is open source.
