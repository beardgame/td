using Bearded.TD.Utilities.Collections;
using Newtonsoft.Json;

namespace Bearded.TD.Utilities
{
    static class JsonConverterCollectionExtensions
    {
        public static void AddRange(this JsonConverterCollection collection, params JsonConverter[] converters)
        {
            converters.ForEach(collection.Add);
        }
    }
}
