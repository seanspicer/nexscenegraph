using System;

namespace Veldrid.SceneGraph.Logging
{
    public interface IVeldridSceneGraphLogger
    {
        void Info(Func<string> buildLogMessage);
        void Debug(Func<string> buildLogMessage);
        void Warn(Func<string> buildLogMessage);
        void Error(Func<string> buildLogMessage);
        void Fatal(Func<string> buildLogMessage);
        void Verbose(Func<string> buildLogMessage);
    }
}