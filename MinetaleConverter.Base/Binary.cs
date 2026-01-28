using System;
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

        public static Dictionary<Type, (int len, Func<byte[], object> parser)> ReadMatrix = new Dictionary<Type, (int, Func<byte[], object>)>()
        {
            { typeof(int), (4, (x) => BitConverter.ToInt32(x)) },
            { typeof(uint), (4, (x) => BitConverter.ToUInt32(x)) },
            { typeof(byte), (1, (x) => x[0]) },
            { typeof(string), (-1, (x) => Encoding.ASCII.GetString(x)) },
            { typeof(double), (8, (x) => BitConverter.ToDouble(x)) },
            { typeof(bool), (1, (x) => BitConverter.ToBoolean(x)) },
            { typeof(long), (8, (x) => BitConverter.ToInt64(x)) },
            { typeof(DateTime), (8, (x) => new DateTime(BitConverter.ToInt64(x))) },
            { typeof(short), (2, (x) => BitConverter.ToInt16(x)) },
            { typeof(ushort), (2, (x) => BitConverter.ToUInt16(x)) }
        };

        public static Dictionary<Type, Func<object, byte[]>> WriteMatrix = new Dictionary<Type, Func<object, byte[]>>()
        {
            { typeof(int), x => BitConverter.GetBytes((int)x) },
            { typeof(uint), x => BitConverter.GetBytes((uint)x) },
            { typeof(byte), x => [(byte)x] },
            { typeof(string), x => Encoding.UTF8.GetBytes(x.ToString() ?? "") },
            { typeof(double), x => BitConverter.GetBytes((double)x) },
            { typeof(bool), x => BitConverter.GetBytes((bool)x) },
            { typeof(long), x => BitConverter.GetBytes((long)x) },
            { typeof(DateTime), x => BitConverter.GetBytes(((DateTime)x).Ticks) },
            { typeof(short), x => BitConverter.GetBytes((short)x) },
            { typeof(ushort), x => BitConverter.GetBytes((ushort)x) },
            { typeof(byte[]), x => (byte[])x }
        };

        public int Length => Bytes.Count();

        public Binary(byte[] arr)
        {
            Bytes = new List<byte>(arr);
        }

        public Binary()
        {
            
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
            (int len, Func<byte[], object> parser) = ReadMatrix[t];
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
            (_, Func<byte[], object> parser) = ReadMatrix[t];
            byte[] b = Subset(Seek, len);
            Seek += len;
            return parser(b);
        }

        public T ReadUntilNull<T>(int index = -1, bool seek = true) => (T)Convert.ChangeType(ReadUntilNull(typeof(T), index, seek), typeof(T));

        public object ReadUntilNull(Type t, int index = -1, bool seek = true)
        {
            if (index >= 0)
                Seek = index;
            (_, Func<byte[], object> parser) = ReadMatrix[t];
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
            (_, Func<byte[], object> parser) = ReadMatrix[t];
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

            byte[] bytes = WriteMatrix[t](obj);
            foreach (var b in bytes)
            {
                while (Seek >= Length)
                    Bytes.Add(0);
                Bytes[Seek] = b;
                Seek++;
            }
        }
    }
}
