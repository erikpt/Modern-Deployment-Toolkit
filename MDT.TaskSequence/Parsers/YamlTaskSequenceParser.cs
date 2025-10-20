using MDT.Core.Interfaces;
using MDT.Core.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MDT.TaskSequence.Parsers;

public class YamlTaskSequenceParser : ITaskSequenceParser
{
    private readonly IDeserializer _deserializer;
    private readonly ISerializer _serializer;

    public YamlTaskSequenceParser()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    public bool CanParse(string content)
    {
        try
        {
            var obj = _deserializer.Deserialize<Dictionary<string, object>>(content);
            return obj.ContainsKey("name") || obj.ContainsKey("steps");
        }
        catch
        {
            return false;
        }
    }

    public Core.Models.TaskSequence Parse(string content)
    {
        return _deserializer.Deserialize<Core.Models.TaskSequence>(content);
    }

    public string Serialize(Core.Models.TaskSequence taskSequence)
    {
        return _serializer.Serialize(taskSequence);
    }
}
