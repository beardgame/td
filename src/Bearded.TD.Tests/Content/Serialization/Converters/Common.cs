using System.IO;
using Newtonsoft.Json;

namespace Bearded.TD.Tests.Content.Serialization.Converters;

static class Common
{
    public static JsonReader StaticStringReader(string s) => new JsonTextReader(new StringReader(s));
}
