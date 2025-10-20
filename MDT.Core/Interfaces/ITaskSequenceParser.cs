using MDT.Core.Models;

namespace MDT.Core.Interfaces;

public interface ITaskSequenceParser
{
    TaskSequence Parse(string content);
    string Serialize(TaskSequence taskSequence);
    bool CanParse(string content);
}
