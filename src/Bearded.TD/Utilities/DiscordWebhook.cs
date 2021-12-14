using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bearded.TD.Utilities;

sealed class DiscordWebhook
{
    private readonly string url;

    public DiscordWebhook(string token)
    {
        url = $"https://discord.com/api/webhooks/{token}";
    }

    public Task SendFileAsync(MemoryStream fileStream, string filename)
    {
        return Task.Run(async () =>
        {
            var form = new MultipartFormDataContent
            {
                {new ByteArrayContent(fileStream.ToArray()), "file", filename}
            };

            using var http = new HttpClient();
            var response = await http.PostAsync(url, form);

            response.EnsureSuccessStatusCode();
        });
    }
}