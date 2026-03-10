using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base
{
    public class Binary
    {
        public List<byte> Bytes { get; private set; } = new List<byte>();
        public EndianMode Endian { get; set; }
        public int Seek
        {
            get => _seek;
            set
            {
                if (value < 0)
                    _seek = 0;
                else
                    _seek = value;
            }
        }
        private int _seek = 0;

        public byte Byte
        {
            get
            {
                if (EOF)
                    return Bytes[Length - 1];
                else
                    return Bytes[Seek];
            }
        }

        public bool EOF => Seek >= Length;

        public static Dictionary<Type, (int len, Func<byte[], object> parser)> ReadMatrixLE = new Dictionary<Type, (int, Func<byte[], object>)>()
        {
            { typeof(int), (4, (x) => BinaryPrimitives.ReadInt32LittleEndian(x)) },
            { typeof(uint), (4, (x) => BinaryPrimitives.ReadUInt32LittleEndian(x)) },
            { typeof(byte), (1, (x) => x[0]) },
            { typeof(string), (-1, (x) => Encoding.ASCII.GetString(x)) },
            { typeof(double), (8, (x) => BinaryPrimitives.ReadDoubleLittleEndian(x)) },
            { typeof(bool), (1, (x) => x[0] > 0) },
            { typeof(long), (8, (x) => BinaryPrimitives.ReadInt64LittleEndian(x)) },
            { typeof(ulong), (8, (x) => BinaryPrimitives.ReadUInt64LittleEndian(x)) },
            { typeof(DateTime), (8, (x) => new DateTime(BinaryPrimitives.ReadInt64LittleEndian(x))) },
            { typeof(short), (2, (x) => BinaryPrimitives.ReadInt16LittleEndian(x)) },
            { typeof(ushort), (2, (x) => BinaryPrimitives.ReadUInt16LittleEndian(x)) }
        };

        public static Dictionary<Type, (int len, Func<byte[], object> parser)> ReadMatrixBE = new Dictionary<Type, (int, Func<byte[], object>)>()
        {
            { typeof(int), (4, (x) => BinaryPrimitives.ReadInt32BigEndian(x)) },
            { typeof(uint), (4, (x) => BinaryPrimitives.ReadUInt32BigEndian(x)) },
            { typeof(byte), (1, (x) => x[0]) },
            { typeof(string), (-1, (x) => Encoding.ASCII.GetString(x)) },
            { typeof(double), (8, (x) => BinaryPrimitives.ReadDoubleBigEndian(x)) },
            { typeof(bool), (1, (x) => x[0] > 0) },
            { typeof(long), (8, (x) => BinaryPrimitives.ReadInt64BigEndian(x)) },
            { typeof(ulong), (8, (x) => BinaryPrimitives.ReadUInt64BigEndian(x)) },
            { typeof(DateTime), (8, (x) => new DateTime(BinaryPrimitives.ReadInt64BigEndian(x))) },
            { typeof(short), (2, (x) => BinaryPrimitives.ReadInt16BigEndian(x)) },
            { typeof(ushort), (2, (x) => BinaryPrimitives.ReadUInt16BigEndian(x)) }
        };

        public static Dictionary<Type, Func<object, byte[]>> WriteMatrixLE = new Dictionary<Type, Func<object, byte[]>>()
        {
            { typeof(int), (x) => WriteWithPrimitive((int)x, 4, (v, x) => BinaryPrimitives.WriteInt32LittleEndian(x, v)) },
            { typeof(uint), (x) => WriteWithPrimitive((uint)x, 4, (v, x) => BinaryPrimitives.WriteUInt32LittleEndian(x, v)) },
            { typeof(byte), (x) => [(byte)x] },
            { typeof(string), (x) => Encoding.UTF8.GetBytes(x.ToString() ?? "") },
            { typeof(double), (x) => WriteWithPrimitive((double)x, 8, (v, x) => BinaryPrimitives.WriteDoubleLittleEndian(x, v)) },
            { typeof(bool), (x) => [(byte)x] }, // revisit?
            { typeof(long), (x) => WriteWithPrimitive((long)x, 8, (v, x) => BinaryPrimitives.WriteInt64LittleEndian(x, v)) },
            { typeof(ulong), (x) => WriteWithPrimitive((ulong)x, 8, (v, x) => BinaryPrimitives.WriteUInt64LittleEndian(x, v)) },
            { typeof(DateTime), (x) => WriteWithPrimitive(((DateTime)x).Ticks, 8, (v, x) => BinaryPrimitives.WriteInt64LittleEndian(x, v)) },
            { typeof(short), (x) => WriteWithPrimitive((short)x, 2, (v, x) => BinaryPrimitives.WriteInt16LittleEndian(x, v)) },
            { typeof(ushort), (x) => WriteWithPrimitive((ushort)x, 2, (v, x) => BinaryPrimitives.WriteUInt16LittleEndian(x, v)) },
            { typeof(byte[]), (x) => (byte[])x }
        };

        public static Dictionary<Type, Func<object, byte[]>> WriteMatrixBE = new Dictionary<Type, Func<object, byte[]>>()
        {
            { typeof(int), (x) => WriteWithPrimitive((int)x, 4, (v, x) => BinaryPrimitives.WriteInt32BigEndian(x, v)) },
            { typeof(uint), (x) => WriteWithPrimitive((uint)x, 4, (v, x) => BinaryPrimitives.WriteUInt32BigEndian(x, v)) },
            { typeof(byte), (x) => [(byte)x] },
            { typeof(string), (x) => Encoding.UTF8.GetBytes(x.ToString() ?? "") },
            { typeof(double), (x) => WriteWithPrimitive((double)x, 8, (v, x) => BinaryPrimitives.WriteDoubleBigEndian(x, v)) },
            { typeof(bool), (x) => [(byte)x] }, // revisit?
            { typeof(long), (x) => WriteWithPrimitive((long)x, 8, (v, x) => BinaryPrimitives.WriteInt64BigEndian(x, v)) },
            { typeof(ulong), (x) => WriteWithPrimitive((ulong)x, 8, (v, x) => BinaryPrimitives.WriteUInt64BigEndian(x, v)) },
            { typeof(DateTime), (x) => WriteWithPrimitive(((DateTime)x).Ticks, 8, (v, x) => BinaryPrimitives.WriteInt64BigEndian(x, v)) },
            { typeof(short), (x) => WriteWithPrimitive((short)x, 2, (v, x) => BinaryPrimitives.WriteInt16BigEndian(x, v)) },
            { typeof(ushort), (x) => WriteWithPrimitive((ushort)x, 2, (v, x) => BinaryPrimitives.WriteUInt16BigEndian(x, v)) },
            { typeof(byte[]), (x) => (byte[])x }
        };

        public int Length => Bytes.Count();

        public Binary(byte[] arr, EndianMode endian = EndianMode.Little)
        {
            Bytes = new List<byte>(arr);
            Endian = endian;
        }

        public Binary(EndianMode endian = EndianMode.Little)
        {
            Endian = endian;
        }

        public byte[] Subset(int index, int count)
        {
            if (index < 0 || index >= Length)
                //return ((byte)0).Stretch(count);
                throw new ArgumentOutOfRangeException();
            if (index + count >= Length)
                count = Length - index;
            return Bytes[index..(index + count)].ToArray();
        }

        public byte[] Subset(int count, bool seek = true)
        {
            var data = Subset(Seek, count);
            if (seek)
                Seek += count;
            return data;
        }

        public void Cut(int index = -1, int count = -1)
        {
            if (index == -1)
                index = Seek;
            if (count == -1)
                count = Length - index;
            Bytes = Bytes[index..(index + count)];
            //Bytes = Subset(index, count).ToList();
            Seek -= index;
        }

        public T Read<T>(int index = -1, bool seek = true) => (T)Convert.ChangeType(Read(typeof(T), index, seek), typeof(T));

        public object Read(Type t, int index = -1, bool seek = true)
        {
            if (index >= 0)
                Seek = index;
            (int len, Func<byte[], object> parser) = (Endian == EndianMode.Little ? ReadMatrixLE[t] : ReadMatrixBE[t]);
            if (len < 0)
                //return ReadUntilNull(t, index, seek);
                return ReadGivenLength(t, typeof(uint), (x) => ((uint)x) - 1);
            else
            {
                byte[] b = Subset(Seek, len);
                Seek += len;
                return parser(b);
            }
        }

        public T ReadLength<T>(int len, int index = -1, bool seek = true) => (T)Convert.ChangeType(ReadLength(typeof(T), len, index, seek), typeof(T));

        public object ReadLength(Type t, int len, int index = -1, bool seek = true)
        {
            if (index >= 0)
                Seek = index;
            (_, Func<byte[], object> parser) = (Endian == EndianMode.Little ? ReadMatrixLE[t] : ReadMatrixBE[t]);
            byte[] b = Subset(Seek, len);
            Seek += len;
            return parser(b);
        }

        public T ReadUntilNull<T>(int index = -1, bool seek = true) => (T)Convert.ChangeType(ReadUntilNull(typeof(T), index, seek), typeof(T));

        public object ReadUntilNull(Type t, int index = -1, bool seek = true)
        {
            if (index >= 0)
                Seek = index;
            (_, Func<byte[], object> parser) = (Endian == EndianMode.Little ? ReadMatrixLE[t] : ReadMatrixBE[t]);
            int ind = Bytes.FindNextIndex(x => x == 0x00, Seek);
            if (ind == -1)
                throw new Exception("Huh?");
            byte[] b = Bytes[Seek..ind].ToArray();
            //byte[] b = Subset(Seek, ind - Seek).ToArray();
            Seek = ind + 1;
            return parser(b);
        }

        public T ReadGivenLength<T, TData>(Func<TData, TData>? transform = null, int index = -1, bool seek = true) => (T)Convert.ChangeType(ReadGivenLength(typeof(T), typeof(TData), transform != null ? (x) => transform((TData)x) : null, index, seek), typeof(T));

        //public object ReadGivenLength(Type t, Type lenDataType, bool swapLengthEndian = false, int index = -1, bool seek = true)
        //{
        //    if (index >= 0)
        //        Seek = index;
        //    index = Seek;
        //    (_, Func<byte[], object> parser) = Matrix[t];
        //    string lenStr = Read(lenDataType).ToString();
        //    int len = int.Parse(lenStr);
        //    if (swapLengthEndian)
        //        len = len.SwapEndian();
        //    object result = ReadLength(t, len);
        //    if (!seek)
        //        Seek = index;
        //    return result;
        //}

        public object ReadGivenLength(Type t, Type lenType, Func<object, object>? transform = null, int index = -1, bool seek = true)
        {
            if (index >= 0)
                Seek = index;
            index = Seek;
            (_, Func<byte[], object> parser) = (Endian == EndianMode.Little ? ReadMatrixLE[t] : ReadMatrixBE[t]);
            object tLen = Read(lenType);
            if (transform != null)
                tLen = transform(tLen);
            string lenStr = tLen.ToString();
            int len = int.Parse(lenStr);
            object result = ReadLength(t, len);
            if (!seek)
                Seek = index;
            return result;
        }

        public void Write<T>(T obj, int index = -1, bool seek = true) => Write(typeof(T), obj, index, seek);

        public void Write(Type t, object? obj, int index = -1, bool seek = true)
        {
            if (index >= 0)
                Seek = index;

            if (obj == null)
                return;

            byte[] bytes = (Endian == EndianMode.Little ? WriteMatrixLE[t](obj) : WriteMatrixBE[t](obj));
            foreach (var b in bytes)
            {
                while (Seek >= Length)
                    Bytes.Add(0);
                Bytes[Seek] = b;
                Seek++;
            }
        }

        public static byte[] WriteWithPrimitive<T>(T val, int length, Action<T, byte[]> writeAction)
        {
            var arr = new byte[length];
            writeAction(val, arr);
            return arr;
        }
    }

    public enum EndianMode
    {
        Little,
        Big
    }
}
