==============
Building Abanu
==============

Abanu can be build platform independent. However, we support
primary an unix build environment. For Windows Users, we offer a
Step by Step guide.

Building on Linux
=================

You need some packages:

* `git` version control system
* `build-essential` Build tools
* `wget` for downloading additional ressorces
* `nasm` Assembler
* `mtools` Creating FAT disk images
* `grub-*` for creating boot images with bootloader. It will not affect your current system. We need the binaries for the disk creation.
* `xorriso` Disk creation

You can install them all via::
  sudo apt-get install -y --no-install-recommends git get nasm qemu-system-x86 mtools xorriso grub-common grub-pc-bin grub-efi-amd64-bin grub-efi-ia32-bin

If you want to debug Abanu, you also need `gdb`.

Download and build:

.. code-block:: sh

  git clone --recursive https://github.com/abanu-org/abanu.git
  cd abanu
  make

To run the kernel, just execute `./abctl run qemu x86-grub-vbe`.
To debug the kernel, run `./abctl debug qemu-kernel`

Building on Windows
===================

The Quick way
-------------

If you want only start run Abanu, just get the sources, open Abanu.sln in Visual Studio, compile whole solution launch the default project (``Abanu.Tools.Build``). However, this is
only a shortcut. If you want debug Abanu, you may need the following steps.


Install the Windows Subsystem for Linux
---------------------------------------

Before installing any Linux distros for WSL, you must ensure that the "Windows Subsystem for Linux" optional feature is enabled:

1. Open PowerShell as Administrator and run:

.. code-block:: powershell

   Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Windows-Subsystem-Linux

2. Restart your computer when prompted.

Visit Microsoft App Store, and Download the App `Debian <https://www.microsoft.com/en-us/p/debian/9msvkqc78pk6>`__

Populate the Debian System with Packages:

.. code-block:: sh

  # run as root
  apt-get update && apt-get install -y wget
  wget -qO- https://raw.githubusercontent.com/abanu-org/abanu/master/build/debian/install | bash -s

This will take a while. After that, you have a fully featured build environment.

Enabling Graphical Unix Applications
------------------------------------

To launch graphical applications like Geany or Qalculate, you need XLaunch / VcXsrv Windows X Server. We can do this all via command line:

In Windows, install the packet manager `chocolatey <https://chocolatey.org>`__. Open a PowerShell with Administrator rights and run:

.. code-block:: powershell

  Set-ExecutionPolicy Bypass -Scope Process -Force; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

Install the package VcXsrv:

.. code-block:: powershell

  choco install -y vcxsrv

You can launch now `XLaunch` from Start menu. Simply follow the wizard without making changes.

Open the Debian app and finish the install instructions. When command prompt is ready, start a dedicated Terminal:

.. code-block:: sh

  export DISPLAY=:0 && xfce4-terminal &

Now you can run unix applications even with graphical user interface.

Additional Tools for Windows:
-----------------------------

.. code-block:: powershell

  choco install -y git

Share project directory
-----------------------

Because Visual Studio cannot open projects via ``\\$wsl``, you have to place the files on the windows drive and link that folder to the WSL home folder.
Run this commands in a WSL/Debian bash shell:

.. code-block:: sh

  # specify root folder for projects.
  WINPROJDIR=$(cmd.exe /C "echo|set /p=%USERPROFILE%")/Documents/abanu-org
  # normalize windows path
  WINPROJDIR=$(wslpath -w $(wslpath -u $WINPROJDIR))
  # create the windows project root
  cmd.exe /C mkdir $WINPROJDIR
  # create symbolic link
  ln -s $(wslpath -u $WINPROJDIR) ~/
  # Switch to new directory
  cd ~/abanu-org

Now ``/home/<user>/abanu-org`` and ``C:\Users\<user>\Documents\abanu-org`` points to the same directory.

Download and build Abanu
------------------------

.. code-block:: sh

  git clone --recursive https://github.com/abanu-org/abanu.git
  cd abanu
  ./abctl configure packages
  ./abctl build all

Now you can run abanu in qemu:

.. code-block:: sh

  ./abctl debug qemu-kernel
