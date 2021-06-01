using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Bearded.Utilities.IO;

namespace Bearded.TD.Utilities
{
    sealed class DiscordWebhook
    {
        private readonly Logger logger;
        private readonly string url;

        public DiscordWebhook(string token, Logger logger)
        {
            this.logger = logger;
            url = $"https://discord.com/api/webhooks/{token}";
        }

        public void SendImageInBackground(Bitmap bitmap)
        {
            Task.Run(async () =>
            {
                try
                {
                    var stream = new MemoryStream();
                    bitmap.Save(stream, ImageFormat.Png);

                    var form = new MultipartFormDataContent
                    {
                        {new ByteArrayContent(stream.ToArray()), "file",
                            $"screenshot-{DateTime.Now:yyyy-dd-M--HH-mm-ss}.png"}
                    };

                    using var http = new HttpClient();
                    var response = await http.PostAsync(url, form);

                    response.EnsureSuccessStatusCode();

                    logger.Debug?.Log("Screenshot sent to Discord successfully");
                }
                catch (Exception e)
                {
                    logger.Error?.Log($"Failed to send image to Discord: {e}");
                }
            });
        }
    }
}
