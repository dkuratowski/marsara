using System;
using System.IO;
using RC.Common;
using RC.Common.Diagnostics;
using RC.Common.Configuration;
using System.Reflection;

namespace RC.NetworkingSystem
{
    class Program
    {
        static Random rnd = new Random();

        static string[] strCollection = new string[10] { "felkeltettem a Krisztit",
                                                             "ijknop",
                                                             "qrstuvwx",
                                                             "",
                                                             "ghijklmn",
                                                             "opqv",
                                                             "wxyzabcd",
                                                             "",
                                                             "mnpqrst",
                                                             "uvyzab" };

        static void Main(string[] args)
        {
            ConfigurationManager.Initialize("../../../../config/RC.NetworkingSystem.TestConsole/RC.NetworkingSystem.TestConsole.root");

            RCPackageFormat f0 = new RCPackageFormat();
            RCPackageFormat f1 = new RCPackageFormat();
            RCPackageFormat f2 = new RCPackageFormat();

            f0.DefineField(RCPackageFieldType.SHORT);
            f0.DefineField(RCPackageFieldType.SHORT_ARRAY);
            f0.DefineField(RCPackageFieldType.STRING_ARRAY);
            f0.DefineField(RCPackageFieldType.STRING);
            f0.DefineField(RCPackageFieldType.INT);
            f0.DefineField(RCPackageFieldType.INT_ARRAY);

            f1.DefineField(RCPackageFieldType.LONG);
            f1.DefineField(RCPackageFieldType.LONG_ARRAY);
            f1.DefineField(RCPackageFieldType.BYTE_ARRAY);
            f1.DefineField(RCPackageFieldType.BYTE);
            f1.DefineField(RCPackageFieldType.STRING);

            f2.DefineField(RCPackageFieldType.BYTE);
            f2.DefineField(RCPackageFieldType.BYTE_ARRAY);
            f2.DefineField(RCPackageFieldType.LONG_ARRAY);
            f2.DefineField(RCPackageFieldType.LONG);
            f2.DefineField(RCPackageFieldType.INT);
            f2.DefineField(RCPackageFieldType.STRING_ARRAY);

            RCPackageFormat.RegisterFormat(f0);
            RCPackageFormat.RegisterFormat(f1);
            RCPackageFormat.RegisterFormat(f2);

            /// WRITING
            TextWriter writer = new StreamWriter("write.txt");
            byte[] buffer = new byte[10000000];
            int pos = 0;
            for (int i = 0; i < 10000; i++)
            {
                RCPackage newPackage = GenerateRandomPackage();
                TraceManager.WriteAllTrace(string.Format("{0}: {1}", i, newPackage.ToString()), TraceManager.GetTraceFilterID("RC.NetworkingSystem.TestConsole.Info"));
                writer.WriteLine(i + ": " + newPackage.ToString());
                pos += newPackage.WritePackageToBuffer(buffer, pos);
            }
            writer.Close();
            /// WRITING BINARY DATA0
            FileStream binStr = new FileStream("bin0.txt", FileMode.Create);
            binStr.Write(buffer, 0, pos);
            binStr.Close();

            /// READING
            pos = 0;
            int parsedBytes = 0;
            RCPackage currPackage = null;
            int count = 0;
            writer = new StreamWriter("read.txt");
            byte[] buffer2 = new byte[10000000];
            int pos2 = 0;

            while (true)
            {
                int burst = rnd.Next(1, 50);
                pos += parsedBytes;
                currPackage = RCPackage.Parse(buffer, pos, burst, out parsedBytes);
                if (currPackage != null)
                {
                    bool error = false;
                    while (!currPackage.IsCommitted)
                    {
                        pos += parsedBytes;
                        burst = rnd.Next(1, 50);
                        if (!currPackage.ContinueParse(buffer, pos, burst, out parsedBytes))
                        {
                            TraceManager.WriteAllTrace("Syntax error", TraceManager.GetTraceFilterID("RC.NetworkingSystem.TestConsole.Info"));
                            writer.WriteLine("Syntax error");
                            error = true;
                        }
                    }
                    if (!error)
                    {
                        TraceManager.WriteAllTrace(string.Format("{0}: {1}", count, currPackage.ToString()), TraceManager.GetTraceFilterID("RC.NetworkingSystem.TestConsole.Info"));
                        writer.WriteLine(count + ": " + currPackage.ToString());
                        pos2 += currPackage.WritePackageToBuffer(buffer2, pos2);
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    TraceManager.WriteAllTrace("Syntax error", TraceManager.GetTraceFilterID("RC.NetworkingSystem.TestConsole.Info"));
                    writer.WriteLine("Syntax error");
                    break;
                }
            }
            writer.Close();

            /// WRITING BINARY DATA1
            binStr = new FileStream("bin1.txt", FileMode.Create);
            binStr.Write(buffer2, 0, pos2);
            binStr.Close();

            TraceManager.WriteAllTrace("READY", TraceManager.GetTraceFilterID("RC.NetworkingSystem.TestConsole.Info"));
            Console.ReadLine();
        }

        public static void ReadBuffer(byte[] buffer)
        {
            int pos = 0;
            int parsedBytes = 0;
            RCPackage currPackage = null;

            while (true)
            {
                pos += parsedBytes;
                currPackage = RCPackage.Parse(buffer, pos, 4, out parsedBytes);
                if (currPackage != null)
                {
                    while (!currPackage.IsCommitted)
                    {
                        pos += parsedBytes;
                        if (!currPackage.ContinueParse(buffer, pos, 4, out parsedBytes))
                        {
                            TraceManager.WriteAllTrace("Syntax error", TraceManager.GetTraceFilterID("RC.NetworkingSystem.TestConsole.Info"));
                            return;
                        }
                    }
                    TraceManager.WriteAllTrace("Package arrived", TraceManager.GetTraceFilterID("RC.NetworkingSystem.TestConsole.Info"));
                }
                else
                {
                    TraceManager.WriteAllTrace("Syntax error", TraceManager.GetTraceFilterID("RC.NetworkingSystem.TestConsole.Info"));
                    return;
                }
            }
        }

        public static RCPackage GenerateRandomPackage()
        {
            int rndType = rnd.Next(0, 3);
            int rndFormat = rnd.Next(0, 3);
            RCPackage retPack = null;
            if (rndType == 0) { retPack = RCPackage.CreateNetworkPingPackage(); return retPack; }
            else if (rndType == 1) { retPack = RCPackage.CreateCustomDataPackage(rndFormat); }
            else if (rndType == 2) { retPack = RCPackage.CreateNetworkCustomPackage(rndFormat); }

            RCPackageFormat format = RCPackageFormat.GetPackageFormat(rndFormat);
            for (int i = 0; i < format.NumOfFields; i++)
            {
                RCPackageFieldType datatype = format.GetFieldType(i);
                if (datatype == RCPackageFieldType.BYTE)
                {
                    retPack.WriteByte(i, (byte)rnd.Next(byte.MinValue, byte.MaxValue));
                }
                else if (datatype == RCPackageFieldType.SHORT)
                {
                    retPack.WriteShort(i, (short)rnd.Next(short.MinValue, short.MaxValue));
                }
                else if (datatype == RCPackageFieldType.INT)
                {
                    retPack.WriteInt(i, (int)rnd.Next(int.MinValue, int.MaxValue));
                }
                else if (datatype == RCPackageFieldType.LONG)
                {
                    retPack.WriteLong(i, (long)rnd.Next(int.MinValue, int.MaxValue));
                }
                else if (datatype == RCPackageFieldType.STRING)
                {
                    int strIdx = rnd.Next(0, 10);
                    retPack.WriteString(i, strCollection[strIdx]);
                }
                else if (datatype == RCPackageFieldType.BYTE_ARRAY)
                {
                    int arrLen = rnd.Next(0, 10);
                    byte[] arr = new byte[arrLen];
                    rnd.NextBytes(arr);
                    retPack.WriteByteArray(i, arr);
                }
                else if (datatype == RCPackageFieldType.SHORT_ARRAY)
                {
                    int arrLen = rnd.Next(0, 10);
                    short[] arr = new short[arrLen];
                    for (int j = 0; j < arrLen; ++j)
                    {
                        arr[j] = (short)rnd.Next(short.MinValue, short.MaxValue);
                    }
                    retPack.WriteShortArray(i, arr);
                }
                else if (datatype == RCPackageFieldType.INT_ARRAY)
                {
                    int arrLen = rnd.Next(0, 10);
                    int[] arr = new int[arrLen];
                    for (int j = 0; j < arrLen; ++j)
                    {
                        arr[j] = (int)rnd.Next(int.MinValue, int.MaxValue);
                    }
                    retPack.WriteIntArray(i, arr);
                }
                else if (datatype == RCPackageFieldType.LONG_ARRAY)
                {
                    int arrLen = rnd.Next(0, 10);
                    long[] arr = new long[arrLen];
                    for (int j = 0; j < arrLen; ++j)
                    {
                        arr[j] = (long)rnd.Next(int.MinValue, int.MaxValue);
                    }
                    retPack.WriteLongArray(i, arr);
                }
                else if (datatype == RCPackageFieldType.STRING_ARRAY)
                {
                    int arrLen = rnd.Next(0, 10);
                    string[] arr = new string[arrLen];
                    for (int j = 0; j < arrLen; ++j)
                    {
                        int strIdx = rnd.Next(0, 10);
                        arr[j] = strCollection[strIdx];
                    }
                    retPack.WriteStringArray(i, arr);
                }
                else { throw new NetworkingSystemException("Unknown datatype"); }
            }

            return retPack;
        }
    }
}
