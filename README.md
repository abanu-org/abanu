## Build instructions
```
git clone --recursive https://github.com/Arakis/lonos.git
cd lonos 
./lonosctl configure patch apply
./lonosctl configure mosa
./lonosctl build all
```
Run it with `./lonosctl run bochs`

## How the image is build / the technology behind this

- Building the compiler. It's pure C#. Requies Mono (Linux) or .NET framework (Windows)
- msbuild / normal compilation of the kernel, pure C# code into a .NET-Assembly.
- Converting the .NET-Assembly with the [Mosa-Compiler](https://github.com/mosa/MOSA-Project) to native machine code
- build some requied Assembler-Code  and append it to the native binary. The Assembler code ist mostly used for early initalization.
- Building the Operating System Disk Image, with Grub2 as Bootloader

## License
Lonos is published under the GNU General Public License. This software includes third party open source software components. Each of these software components have their own license. All sourcecode is open source.
