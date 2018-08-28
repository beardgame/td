using System;
using Fody;
using Mono.Cecil.Cil;

namespace Weavers {
    public interface ILogger {
        Action<string> LogDebug { get; set; }
        Action<string> LogInfo { get; set; }
        Action<string, MessageImportance> LogMessage { get; set; }
        Action<string> LogWarning { get; set; }
        Action<string, SequencePoint> LogWarningPoint { get; set; }
        Action<string> LogError { get; set; }
        Action<string, SequencePoint> LogErrorPoint { get; set; }
    }
}