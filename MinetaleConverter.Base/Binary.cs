using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base
{
    public class Binary
    {
        public byte[] Bytes { get; private set; }
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
                    return Bytes[Bytes.Length - 1];
                else
                    return Bytes[Seek];
            }
        }

        public bool EOF => Seek >= Bytes.Length;

        public static Dictionary<Type, (int len, Func<byte[], object> parser)> Matrix = new Dictionary<Type, (int, Func<byte[], object>)>()
        {
            { typeof(int), (4, (x) => BitConverter.ToInt32(x)) },
            { typeof(byte), (1, (x) => x[0]) },
            { typeof(string), (-1, (x) => Encoding.ASCII.GetString(x)) },
            { typeof(double), (8, (x) => BitConverter.ToDouble(x)) },
            { typeof(bool), (1, (x) => BitConverter.ToBoolean(x)) },
            { typeof(long), (8, (x) => BitConverter.ToInt64(x)) },
            { typeof(DateTime), (8, (x) => new DateTime(BitConverter.ToInt64(x))) }
        };

        public Binary(byte[] arr)
        {
            Bytes = arr;
        }

        public byte[] Subset(int index, int count)
        {
            if (index < 0 || index >= Bytes.Length)
                throw new ArgumentOutOfRangeException();
            if (index + count >= Bytes.Length)
                count = Bytes.Length - index;
            return Bytes[index..(index + count)];
        }

        public byte[] Subset(int count, bool seek = true)
        {
            var data = Subset(Seek, count);
            if (seek)
                Seek += count;
            return data;
        }

        public T Read<T>(int index = -1, bool seek = true) => (T)Convert.ChangeType(Read(typeof(T), index, seek), typeof(T));

        public object Read(Type t, int index = -1, bool seek = true)
        {
            if (index >= 0)
                Seek = index;
            (int len, Func<byte[], object> parser) = Matrix[t];
            byte[] b = Subset(Seek, len);
            Seek += len;
            return parser(b);
        }

        public T ReadLength<T>(int len, int index = -1, bool seek = true) => (T)Convert.ChangeType(ReadLength(typeof(T), len, index, seek), typeof(T));

        public object ReadLength(Type t, int len, int index = -1, bool seek = true)
        {
            if (index >= 0)
                Seek = index;
            (_, Func<byte[], object> parser) = Matrix[t];
            byte[] b = Subset(Seek, len);
            Seek += len;
            return parser(b);
        }

        public T ReadUntilNull<T>(int index = -1, bool seek = true) => (T)Convert.ChangeType(ReadUntilNull(typeof(T), index, seek), typeof(T));

        public object ReadUntilNull(Type t, int index = -1, bool seek = true)
        {
            if (index >= 0)
                Seek = index;
            (_, Func<byte[], object> parser) = Matrix[t];
            int ind = Bytes.FindNextIndex(x => x == 0x00, Seek);
            if (ind == -1)
                throw new Exception("Huh?");
            byte[] b = Bytes[Seek..ind];
            Seek = ind + 1;
            return parser(b);
        }

        public void Cut(int index = -1, int count = -1)
        {
            if (index == -1)
                index = Seek;
            if (count == -1)
                count = Bytes.Length - index;
            Bytes = Bytes[index..(index + count)];
        }
    }
}
