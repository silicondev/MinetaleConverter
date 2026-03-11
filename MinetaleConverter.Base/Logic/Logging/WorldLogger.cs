using MinetaleConverter.Base.Logic.Interfaces;
using MinetaleConverter.Base.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Logic.Logging
{
    public class WorldLogger : Logger
    {
        private IWorld _world;
        private Logger _baseLogger;

        public WorldLogger(IWorld world, Logger baseLogger)
        {
            _world = world;
            _baseLogger = baseLogger;
        }

        public override void Message<T>(LogType type, T state, Func<T, string> messageFunc) =>
            _baseLogger.Message(type, state, (x) => $"[{_world.LevelName}] {messageFunc(x)}");
    }
}
