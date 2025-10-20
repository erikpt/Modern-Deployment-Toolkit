using MDT.Core.Interfaces;
using MDT.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MDT.TaskSequence.Parsers;

public class JsonTaskSequenceParser : ITaskSequenceParser
{
    public bool CanParse(string content)
    {
        try
        {
            var obj = JObject.Parse(content);
            return obj["name"] != null || obj["steps"] != null;
        }
        catch
        {
            return false;
        }
    }

    public Core.Models.TaskSequence Parse(string content)
    {
        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        return JsonConvert.DeserializeObject<Core.Models.TaskSequence>(content, settings) 
               ?? throw new InvalidOperationException("Failed to deserialize task sequence");
    }

    public string Serialize(Core.Models.TaskSequence taskSequence)
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        return JsonConvert.SerializeObject(taskSequence, settings);
    }
}
