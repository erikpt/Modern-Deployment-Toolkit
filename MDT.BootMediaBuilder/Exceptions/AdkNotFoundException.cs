namespace MDT.BootMediaBuilder.Exceptions;

/// <summary>
/// Exception thrown when Windows ADK is not found or not properly installed
/// </summary>
public class AdkNotFoundException : Exception
{
    public AdkNotFoundException(string message) : base(message)
    {
    }
    
    public AdkNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
