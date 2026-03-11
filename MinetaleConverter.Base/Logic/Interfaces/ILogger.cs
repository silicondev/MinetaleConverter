using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Logic.Interfaces
{
    public interface ILogger
    {
        void Message(LogType type, string message);
        void Message<T>(LogType type, T state, Func<T, string> messageFunc);
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Error(string message, Exception exception);
        void Error(Exception exception);
        void Fatal(string message);
        void Fatal(string message, Exception exception);
        void Fatal(Exception exception);
    }

    public enum LogType
    {
        INF,
        WRN,
        ERR,
        FTL
    }
}
