namespace MDT.BootMediaBuilder.Exceptions;

/// <summary>
/// Exception thrown when a boot media build operation fails
/// </summary>
public class BuildFailedException : Exception
{
    public BuildFailedException(string message) : base(message)
    {
    }
    
    public BuildFailedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
