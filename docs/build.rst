##############
Building Lonos
##############

Lonos can be build plattform indepentent. However, we support
primary an unix build environment. For Windows Users, we offer a 
Step by Step guide.

Install the Windows Subsystem for Linux
---------------------------------------

Before installing any Linux distros for WSL, you must ensure that the "Windows Subsystem for Linux" optional feature is enabled:

1. Open PowerShell as Administrator and run:
	
.. code-block:: powershell

	Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Windows-Subsystem-Linux

2. Restart your computer when prompted.

Visit Microsoft App Store, and Download the App `Debian <https://www.microsoft.com/en-us/p/debian/9msvkqc78pk6>`__ 

Populate the Debian System with Packages:

.. code-block:: console

  wget -qO- https://raw.githubusercontent.com/lonos-project/lonos/master/build/debian/install | bash -s

This will take a while. After that, you have a fully featured build environment. To launch graphical applications like Geany or Qalculate, you need XLaunch. We can do this all via command line:

In Windows, install the packetmanager `chocolatey <https://chocolatey.org>`__. Open a PowerShell with Administrator rights and run:

.. code-block:: powershell

  Set-ExecutionPolicy Bypass -Scope Process -Force; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

Install the package xming:

.. code-block:: powershell

  choco install xming

You can launch now `XLaunch` from Start menu. Simply follow the wizard without making changes.

Open the Debian app and finish the install instructions. Whem command prompt is ready, start a dedicated Terminal:

.. code-block:: sh

  export DISPLAY=:0 && xfce4-terminal &

Now you can run unix applications even with graphical user interface.

Download and build Lonos
------------------------

.. code-block:: sh

  git clone --recursive https://github.com/lonos-project/lonos.git
  cd lonos 
  ./lonosctl configure mosa
  ./lonosctl build all

Now you can run lonos in qemu:

.. code-block:: sh

   ./lonosctl debug qemu-kernel



