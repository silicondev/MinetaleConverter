using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.PlayerEntities
{
    public class hy_CompletePositon
    {
        public hy_CompletePositon()
        {
            
        }

        public hy_CompletePositon(Position position, Rotation rotation)
        {
            _position = position;
            _rotation = rotation;
        }

        public hy_CompletePositon(int x, int y, int z, int pitch, int yaw, int roll) : this(new Position(x, y, z), new Rotation(pitch, yaw, roll)) { }

        [JsonIgnore]
        private Position _position = new Position();

        [JsonIgnore]
        private Rotation _rotation = new Rotation();

        public double X
        {
            get => _position.X;
            set => _position.X = value;
        }
        public double Y
        {
            get => _position.Y;
            set => _position.Y = value;
        }
        public double Z
        {
            get => _position.Z;
            set => _position.Z = value;
        }
        public double Pitch
        {
            get => _rotation.Pitch;
            set => _rotation.Pitch = value;
        }
        public double Yaw
        {
            get => _rotation.Yaw;
            set => _rotation.Yaw = value;
        }
        public double Roll
        {
            get => _rotation.Roll;
            set => _rotation.Roll = value;
        }
    }
}
