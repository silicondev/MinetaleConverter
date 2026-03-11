using MinetaleConverter.Base.Logic.Interfaces;
using MinetaleConverter.Base.Logic.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Logic.Logging
{
    public class ConsoleLogger : Logger
    {
        public override void Message<T>(LogType type, T state, Func<T, string> messageFunc) =>
            Console.WriteLine($"{DateTime.Now:dd/MM/yyyy HH:mm:ss.fff} | {Enum.GetName(type)} | {messageFunc(state)}");
    }
}
