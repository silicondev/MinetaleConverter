using MinetaleConverter.Base.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Logic.Logging
{
    public abstract class Logger : ILogger
    {
        public abstract void Message<T>(LogType type, T state, Func<T, string> messageFunc);

        public void Message(LogType type, string message) =>
            Message(type, message, (x) => x);

        public void Info(string message) =>
            Message(LogType.INF, message);

        public void Warn(string message) =>
            Message(LogType.WRN, message);

        public void Error(string message) =>
            Message(LogType.ERR, message);

        public void Error(string message, Exception exception) =>
            Message(LogType.ERR, exception, (x) => $"{message} : {x.Message}{Environment.NewLine}{x.StackTrace}");

        public void Error(Exception exception) =>
            Message(LogType.ERR, exception, (x) => $"{x.Message}{Environment.NewLine}{x.StackTrace}");

        public void Fatal(string message) =>
            Message(LogType.FTL, message);

        public void Fatal(string message, Exception exception) =>
            Message(LogType.FTL, exception, (x) => $"{message} : {x.Message}{Environment.NewLine}{x.StackTrace}");

        public void Fatal(Exception exception) =>
            Message(LogType.FTL, exception, (x) => $"{x.Message}{Environment.NewLine}{x.StackTrace}");
    }
}
