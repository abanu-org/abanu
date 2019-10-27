FROM debian:buster

RUN apt-get update \
    && apt-get upgrade -y

RUN apt-get install -y wget ca-certificates apt-transport-https

RUN apt-get install -y apt-transport-https dirmngr \
    && apt-key adv --no-tty --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb https://download.mono-project.com/repo/debian stable-buster main" | tee /etc/apt/sources.list.d/mono-official-stable.list

RUN wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg \
	&& mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/ \
	&& wget -q https://packages.microsoft.com/config/debian/10/prod.list \
	&& mv prod.list /etc/apt/sources.list.d/microsoft-prod.list \
	&& chown root:root /etc/apt/trusted.gpg.d/microsoft.asc.gpg \
	&& chown root:root /etc/apt/sources.list.d/microsoft-prod.list

RUN apt-get update \
    && apt-get upgrade -y

RUN apt-get install -y qemu-system-x86 git curl mono-complete mtools xorriso nasm build-essential gdb bochs colordiff man rsync git
#grub2-common grub-efi-ia32 grub-efi-amd64 grub-pc grub-rescue-pc
RUN apt-get install -y  dotnet-sdk-2.2

RUN mkdir /abanu
