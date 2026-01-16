namespace MDT.BootMediaBuilder.Exceptions;

/// <summary>
/// Exception thrown when a DISM operation fails
/// </summary>
public class DismOperationException : Exception
{
    public string? DismOutput { get; set; }
    
    public DismOperationException(string message) : base(message)
    {
    }
    
    public DismOperationException(string message, string dismOutput) : base(message)
    {
        DismOutput = dismOutput;
    }
    
    public DismOperationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
