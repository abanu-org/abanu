using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Lonos.HostServer
{
    class Program
    {
        private static NetworkStream stream;
        private static Thread conThread;
        private static Thread thRead;
        private static Thread thWrite;
        static void Main(string[] args)
        {
            var th = new Thread(ConnThread);
            th.Start();

            while (true)
            {
                var line = Console.ReadLine();
                if (line == "q" || line == "quit")
                    Environment.Exit(0);

                PostMessge(Encoding.ASCII.GetBytes("CMD:" + line));
            }
        }

        private static AutoResetEvent ConThreadWaiter = new AutoResetEvent(true);
        public static void ConnThread()
        {
            while (true)
            {
                ConThreadWaiter.WaitOne();
                IsConnecting = true;
                Start();
                IsConnecting = false;
            }
        }

        public static bool IsConnecting = true;

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void Restart()
        {
            if (IsConnecting)
                return;
            IsConnecting = true;
            ConThreadWaiter.Set();
        }

        private static TcpClient client;

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void Start()
        {
            while (true)
            {
                try
                {
                    stream?.Dispose();
                }
                catch { }
                try
                {
                    client?.Dispose();
                }
                catch { }
                try
                {
                    thRead?.Abort();
                }
                catch { }
                try
                {
                    thWrite?.Abort();
                }
                catch { }
                thRead = null;
                thWrite = null;

                expectedLength = 0;
                ms.SetLength(0);
                WriteQueue.Clear();

                try
                {
                    client = new TcpClient();
                    Console.WriteLine("Connecting...");
                    client.Connect("localhost", 2244);
                    Console.WriteLine("Connected");
                    receiveBufSize = client.ReceiveBufferSize;
                    stream = client.GetStream();

                    thRead = new Thread(ReadThread);
                    thRead.Start();

                    thWrite = new Thread(WriteThread);
                    thWrite.Start();

                    IsConnecting = false;

                    break;
                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex);
                    Thread.Sleep(3000);
                }
            }
        }

        private static Queue<byte[]> WriteQueue = new Queue<byte[]>();
        private static AutoResetEvent WriteWaiter = new AutoResetEvent(false);

        private static void WriteThread()
        {
            try
            {
                while (true)
                {
                    WriteWaiter.WaitOne();

                    while (true)
                    {

                        byte[] data;
                        lock (WriteQueue)
                        {
                            if (WriteQueue.Count == 0)
                                break;
                            data = WriteQueue.Dequeue();
                        }

                        stream.Write(data, 0, data.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!IsConnecting)
                    Restart();
            }
        }

        public static void PostMessge(byte[] msg)
        {
            lock (WriteQueue)
                WriteQueue.Enqueue(msg);
            WriteWaiter.Set();
        }

        private static int receiveBufSize = 1000;

        private static void ReadThread()
        {
            var reader = new BinaryReader(stream);
            try
            {
                while (true)
                {
                    var lineType = reader.ReadByte();
                    switch (lineType)
                    {
                        case 200:
                            var msgId = reader.ReadInt32();
                            var command = reader.ReadInt32();
                            HeaderReceived(msgId, command);
                            break;
                        case 201:
                            var length = reader.ReadInt32(); ;
                            var data = reader.ReadBytes(length);
                            ArgReceived(data);
                            break;
                        case 202:
                            var length2 = reader.ReadInt32(); ;
                            var data2 = reader.ReadBytes(length2);
                            DataReceived(data2);
                            break;
                        case 203:
                            EndReceived();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!IsConnecting)
                    Restart();
            }
        }

        private static MemoryStream ms = new MemoryStream();
        private static int expectedLength = 0;

        private static int CurrentCommand;
        private static int MessageId;
        static void HeaderReceived(int msgId, int command)
        {
            CurrentCommand = command;
            MessageId = msgId;
        }

        static List<byte[]> Args = new List<byte[]>();

        static void ArgReceived(byte[] data)
        {
            Args.Add(data);
        }

        static void DataReceived(byte[] data)
        {
            ms.Write(data, 0, data.Length);
        }

        static void EndReceived()
        {
            var data = ms.ToArray();
            var args = Args.ToArray();
            Args.Clear();
            ms.SetLength(0);
            MessageReceived(MessageId, CurrentCommand, args, data);
        }

        public static void MessageReceived(int msgId, int command, byte[][] args, byte[] data)
        {
            switch (command)
            {
                case 240:
                    CmdOpenFile(msgId, args, data);
                    break;
            }
        }

        public static void CmdOpenFile(int msgId, byte[][] args, byte[] data)
        {
            var txt = Encoding.ASCII.GetString(args[0]);
            Console.WriteLine(txt);
            WriteResult(msgId, 0x0A0B0C0D);
        }

        public static void WriteResult(int msgId, int result)
        {
            WriteResult(msgId, BitConverter.GetBytes(result));
        }

        public static void WriteResult(int msgId, byte[] result)
        {
            PostMessge(new byte[] { 204 });
            PostMessge(BitConverter.GetBytes(msgId));
            PostMessge(result);
        }

    }
}
