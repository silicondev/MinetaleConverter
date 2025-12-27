using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Base.Logging
{
    public interface ILogger
    {
        void Info(string message);
        void Warn(string message);
        void Warn(string message, Exception e);
        void Warn(Exception e);
        void Error(string message);
        void Error(string message, Exception e);
        void Error(Exception e);
        void Critical(string message);
        void Critical(string message, Exception e);
        void Critical(Exception e);
    }
}
