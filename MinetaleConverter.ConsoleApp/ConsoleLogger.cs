using MinetaleConverter.Base.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.ConsoleApp
{
    public class ConsoleLogger : ILogger
    {
        public void Critical(string message) =>
            SendMessage("CRT", message);

        public void Critical(string message, Exception e) =>
            SendMessage("CRT", $"{message} : {e.Message}{Environment.NewLine}{e.StackTrace}");

        public void Critical(Exception e) =>
            SendMessage("CRT", $"{e.Message}{Environment.NewLine}{e.StackTrace}");

        public void Error(string message) =>
            SendMessage("ERR", message);

        public void Error(string message, Exception e) =>
            SendMessage("ERR", $"{message} : {e.Message}");

        public void Error(Exception e) =>
            SendMessage("ERR", e.Message);

        public void Info(string message) =>
            SendMessage("INF", message);

        public void Warn(string message) =>
            SendMessage("WRN", message);

        public void Warn(string message, Exception e) =>
            SendMessage("WRN", $"{message} : {e.Message}");

        public void Warn(Exception e) =>
            SendMessage("WRN", e.Message);

        private void SendMessage(string prefix, string message) =>
            Console.WriteLine($"{DateTime.UtcNow:yyyyMMdd HH:mm:ss.fffffff} [{prefix}] {message}");
    }
}
