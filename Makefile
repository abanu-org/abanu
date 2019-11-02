CUR_DIR = $(CURDIR)

KERNEL_OUT := os/Abanu.OS.Core.x86.bin
LOADER_OUT := os/Abanu.OS.Loader.x86.bin
IMAGE_OUT := os/Abanu.OS.Image.x86.bin
HELLOKERNEL_OUT := os/App.HelloKernel.bin
HELLOSERVICE_OUT := os/App.HelloService.bin
CONSOLESERVER_OUT := os/Abanu.Service.ConsoleServer.bin
SERVICE_BASIC_OUT := os/Abanu.Service.Basic.bin
SERVICE_HOSTCOMMUNICATION_OUT := os/Abanu.Service.HostCommunication.bin
APP_SHELL_OUT := os/App.Shell.bin

KERNEL_NET := bin/Abanu.Kernel.Core.x86.dll
LOADER_NET := bin/Abanu.Kernel.Loader.x86.dll
HELLOKERNEL_NET := bin/App.HelloKernel.exe
HELLOSERVICE_NET := bin/App.HelloService.exe
CONSOLESERVER_NET := bin/Abanu.Service.ConsoleServer.exe
SERVICE_BASIC_NET := bin/Abanu.Service.Basic.exe
SERVICE_HOSTCOMMUNICATION_NET := bin/Abanu.Service.HostCommunication.exe
APP_SHELL_NET := bin/App.Shell.exe

NATIVE := bin/x86

KERNEL_EFI_DISK := os/Abanu.Kernel.Core.x86-efi.disk.img
KERNEL_HYBRID_DISK := os/Abanu.Kernel.Core.x86-grub-hybrid.disk.img

# virtual targets

# all: net out
all:
	$(MAKE) NET
	$(MAKE) out

out: $(KERNEL_EFI_DISK)

rebuild:
	$(MAKE) -B

native: $(NATIVE)

# .PHONY: all net out
.PHONY: all

# file targets

$(NATIVE):
	./abctl build native

$(IMAGE_OUT): $(LOADER_OUT) $(KERNEL_OUT)
	./abctl build image

$(LOADER_OUT): $(LOADER_NET) $(NATIVE)
	./abctl build loader

$(KERNEL_OUT): $(KERNEL_NET) $(HELLOKERNEL_OUT) $(HELLOSERVICE_OUT) $(CONSOLESERVER_OUT) $(SERVICE_BASIC_OUT) $(SERVICE_HOSTCOMMUNICATION_OUT) $(APP_SHELL_OUT) $(NATIVE)
	./abctl build kernel

$(HELLOKERNEL_OUT): $(HELLOKERNEL_NET) $(NATIVE)
	./abctl build app

$(HELLOSERVICE_OUT): $(HELLOSERVICE_NET) $(NATIVE)
	./abctl build app2


$(CONSOLESERVER_OUT): $(CONSOLESERVER_NET) $(NATIVE)
	./abctl build service.consoleserver

$(SERVICE_BASIC_OUT): $(SERVICE_BASIC_NET) $(NATIVE)
	./abctl build service.basic

$(SERVICE_HOSTCOMMUNICATION_OUT): $(SERVICE_HOSTCOMMUNICATION_NET) $(NATIVE)
	./abctl build service.hostcommunication

$(APP_SHELL_OUT): $(APP_SHELL_NET) $(NATIVE)
	./abctl build app.shell


#$(KERNEL_NET) $(LOADER_NET) $(HELLOKERNEL_NET) $(HELLOSERVICE_NET) $(CONSOLESERVER_NET) $(SERVICE_BASIC_NET) $(SERVICE_HOSTCOMMUNICATION_NET) $(APP_SHELL_NET): net

external/MOSA-Project/bin: external/MOSA-Project/Source/packages
	./abctl configure mosa

external/MOSA-Project/Source/packages:
	./abctl configure packages

$(KERNEL_NET) $(LOADER_NET) $(HELLOKERNEL_NET) $(HELLOSERVICE_NET) $(CONSOLESERVER_NET) $(SERVICE_BASIC_NET) $(SERVICE_HOSTCOMMUNICATION_NET) $(APP_SHELL_NET):
	$(MAKE) NET

$(KERNEL_EFI_DISK) $(KERNEL_HYBRID_DISK): $(IMAGE_OUT)
	./abctl build disk

NET:
	$(MAKE) external/MOSA-Project/bin
	./abctl build assembly