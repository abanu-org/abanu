// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using Mosa.DeviceSystem;
using Mosa.FileSystem;
using Mosa.FileSystem.VFS;

namespace Abanu.Kernel.Core
{
    public enum VolumeDescriptorType : byte
    {
        Boot = 0,
        Primary = 1,
        Supplementary = 2,
        Partition = 3,
        SetTerminator = 255,
    }

    public struct VolumeDescriptor
    {
        public byte[] Data;
        public byte[] Identifier;
        public byte Type;
        public byte Version;
    }

    public class Iso9660FileSystem : GenericFileSystem, IFileSystem
    {
        public VolumeDescriptor[] DescriptorSet { get; set; }
        public bool IsReadOnly => false;
        public IVfsNode Root => _root;

        public Iso9660FileSystem(IPartitionDevice device)
            : base(device)
        {
        }

        public override IFileSystem CreateVFSMount()
        {
            return this;
        }

        private IVfsNode _root;
    }
}
