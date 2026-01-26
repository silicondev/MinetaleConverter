using MinetaleConverter.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Bson
{
    public class BsonFile
    {
        public Binary? Binary { get; private set; }

        public bool Ready => Binary != null;

        public Dictionary<string, (BsonType type, object data)> Data { get; private set; } = new Dictionary<string, (BsonType type, object data)>();

        public BsonFile(byte[] bsonData)
        {
            Import(bsonData);
        }

        public BsonFile(Dictionary<string, (BsonType type, object data)> data)
        {
            Data = data;
        }

        public bool Import(byte[] bytes, int docSize = -1)
        {
            Binary = new Binary(bytes);
            Data = parse();
            return true;
        }

        public override string ToString() => ToString(Data, 0);
        public string ToString(Dictionary<string, (BsonType type, object data)> dict, int tab)
        {
            string str = "";
            foreach (var kvp in dict)
            {
                str += $"{tab.ToCharString(' ')}{kvp.Key}: ";
                (BsonType type, object val) = kvp.Value;
                switch (type)
                {
                    case BsonType.DOCUMENT:
                        var newDict = ((BsonFile)val).Data;
                        str += $"{Environment.NewLine}{(tab + 1).ToCharString(' ')}\\_{Environment.NewLine}";
                        str += ToString(newDict, tab + 4);
                        break;
                    case BsonType.OBJECT_ID:
                    case BsonType.BINARY:
                        byte[] bytes = (byte[])val;
                        str += Encoding.ASCII.GetString(bytes) + Environment.NewLine;
                        break;
                    case BsonType.ARRAY:
                        object[] objects = (object[])val;
                        str += objects.ToArrayString() + Environment.NewLine;
                        break;
                    case BsonType.STRING:
                        str += $"\"{val}\"{Environment.NewLine}";
                        break;
                    default:
                        str += val.ToString() + Environment.NewLine;
                        break;
                }
            }
            return str;
        }

        public T Get<T>(string name)
        {
            var type = typeof(T);
            (BsonType bsonType, object data) = Data[name];
            return (T)data;
            //if (type == typeof(BsonFile))
            //    return (T)data;

            //return (T)Convert.ChangeType(data, type);
        }

        private Dictionary<string, (BsonType type, object data)> parse()
        {
            var data = new Dictionary<string, (BsonType type, object data)>();
            if (!Ready)
                return data;

            int start = Binary!.Seek;
            int docSize = Binary.Read<int>();
            while (Binary.Seek < start + docSize - 1)
            {
                var type = (BsonType)Binary.Read<byte>();
                if (type == 0)
                    break;
                string name = Binary.ReadUntilNull<string>();
                object val;
                switch (type)
                {
                    case BsonType.DOCUMENT:
                        val = new BsonFile(parse());
                        break;
                    case BsonType.OBJECT_ID:
                        byte[] objectId = Binary.Subset(12);
                        val = objectId;
                        break;
                    case BsonType.BINARY:
                        int binLength = Binary.Read<int>();
                        val = Binary.Subset(binLength + 1);
                        break;
                    case BsonType.ARRAY:
                        var arrayData = parse();
                        var arr = new object[arrayData.Count()];
                        for (int ai = 0; ai < arrayData.Count(); ai++)
                            arr[ai] = arrayData[ai.ToString()];
                        val = arr;
                        break;
                    default:
                        val = Binary.Read(_bsonMatrix[type]);
                        break;
                }

                data[name] = (type, val);
            }

            if (Binary.Seek < start + docSize)
                Binary.Seek = start + docSize;
            return data;
        }

        private Dictionary<BsonType, Type> _bsonMatrix = new Dictionary<BsonType, Type>()
        {
            { BsonType.DOUBLE, typeof(double) },
            { BsonType.STRING, typeof(string) },
            { BsonType.BOOLEAN, typeof(bool) },
            { BsonType.INT32, typeof(int) },
            { BsonType.INT64, typeof(long) },
            { BsonType.DATETIME, typeof(DateTime) },
            { BsonType.TIMESTAMP, typeof(DateTime) },
            { BsonType.REGEX, typeof(string) }
        };
    }

    public enum BsonType
    {
        DOUBLE = 0x01,
        STRING = 0x02,
        DOCUMENT = 0x03,
        ARRAY = 0x04,
        BINARY = 0x05,
        UNDEFINED = 0x06, // Deprecated
        OBJECT_ID = 0x07,
        BOOLEAN = 0x08,
        DATETIME = 0x09,
        NULL = 0x0A,
        REGEX = 0x0B,
        DBPOINTER = 0x0C, // Deprecated
        JAVASCRIPT = 0x0D,
        SYMBOL = 0x0E, // Deprecated
        CODE_W_SCOPE = 0x0F,
        INT32 = 0x10,
        TIMESTAMP = 0x11,
        INT64 = 0x12,
        DECIMAL128 = 0x13,
        MIN_KEY = 0xFF,
        MAX_KEY = 0x7F
    }
}
