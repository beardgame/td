using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Bearded.TD.Utilities;
using Bearded.Utilities.IO;
using Bearded.Utilities.Threading;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace Bearded.TD.Meta;

sealed class ScreenshotSaver
{
    private readonly Logger logger;
    private readonly IActionQueue glActions;

    public ScreenshotSaver(Logger logger, IActionQueue glActions)
    {
        this.logger = logger;
        this.glActions = glActions;
    }

    public Task SendScreenshotToDiscordAsync(ViewportSize viewport)
    {
        logger.Info?.Log($"Trying to send screenshot ({viewport.Width}x{viewport.Height}) to Discord...");

        // ReSharper disable once AsyncVoidLambda
        return tryTaskRun("Sending screenshot to Discord", async () =>
        {
            var webhookToken = UserSettings.Instance.Debug.DiscordScreenshotWebhookToken;
            if (string.IsNullOrEmpty(webhookToken))
            {
                logger.Warning?.Log("Set webhook token to send screenshot to discord");
                return;
            }

            await using var stream = new MemoryStream();

            using(var bitmap = await makeScreenshotAsync(viewport))
            {
                bitmap.Save(stream, ImageFormat.Png);
            }

            var filename = getScreenshotFileName();

            var webhook = new DiscordWebhook(webhookToken);
            await webhook.SendFileAsync(stream, filename);

            logger.Info?.Log($"Sent screenshot to Discord as {filename}");
        });
    }

    public Task SaveScreenShotAsync(ViewportSize viewport)
    {
        logger.Info?.Log($"Trying to save screenshot ({viewport.Width}x{viewport.Height}) to disk...");

        // ReSharper disable once AsyncVoidLambda
        return tryTaskRun("Saving screenshot", async () =>
        {
            var bitmap = await makeScreenshotAsync(viewport);

            var path = UserSettings.Instance.Misc.ScreenshotPath;

            if (string.IsNullOrEmpty(path))
                path = Path.Combine(Microsoft.VisualBasic.FileIO.SpecialDirectories.MyPictures, "Bearded.TD");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var completePath = Path.Combine(path, getScreenshotFileName());

            bitmap.Save(completePath, ImageFormat.Png);

            bitmap.Dispose();

            logger.Info?.Log($"Saved screenshot to {completePath}");
        });
    }

    private Task<Bitmap> makeScreenshotAsync(ViewportSize viewport)
    {
        var (width, height) = (viewport.Width, viewport.Height);

        return glActions.Run(() =>
        {
            var bmp = new Bitmap(width, height);
            var data = bmp.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.ReadPixels(0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bmp;
        });
    }

    private static string getScreenshotFileName()
    {
        return $"screenshot-{DateTime.Now:yyyy-MM-ddTHHmmss.FFFF}.png";
    }

    private Task tryTaskRun(string actionDescription, Action action)
    {
        return Task.Run(() =>
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                logger.Error?.Log($"{actionDescription} failed: {e}");
            }
        });
    }
}
