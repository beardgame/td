using System;
using System.Diagnostics;
using Bearded.Graphics.ShaderManagement;
using Bearded.Utilities.IO;

namespace Bearded.TD.Rendering
{
    sealed class DebugOnlyShaderReloader
    {
        private readonly ShaderManager shaders;
        private readonly Logger logger;
        private readonly TimeSpan reloadInterval = TimeSpan.FromSeconds(1);

        private DateTime nextShaderReloadTime = DateTime.UtcNow;

        public DebugOnlyShaderReloader(ShaderManager shaders, Logger logger)
        {
            this.shaders = shaders;
            this.logger = logger;
        }

        [Conditional("DEBUG")]
        public void ReloadShadersIfNeeded()
        {
            var now = DateTime.UtcNow;

            if (nextShaderReloadTime > now)
                return;

            nextShaderReloadTime = now + reloadInterval;

            var report = shaders.TryReloadAll();

            logShaderReloadReport(report, now);
        }

        private void logShaderReloadReport(ShaderReloadReport report, DateTime timestamp)
        {
            if (!report.TriedReloadingAnything)
                return;

            logger.Debug?.Log($"Found changed shaders at {timestamp:O}");

            if (report.ReloadedShaderCount > 0)
                logger.Debug?.Log($"Reloaded {report.ReloadedShaderCount} shaders.");

            if (report.ReloadedProgramCount > 0)
                logger.Debug?.Log($"Reloaded {report.ReloadedProgramCount} shader programs.");

            logShaderReloadErrors(report);
        }

        private void logShaderReloadErrors(ShaderReloadReport report)
        {
            if (report.ReloadExceptions.Length <= 0)
                return;

            logger.Error?.Log("Failed to reload shaders:");

            foreach (var exception in report.ReloadExceptions)
            {
                foreach (var line in exception.Message.Split('\n'))
                {
                    logger.Error?.Log(line);
                }
            }
        }
    }
}
