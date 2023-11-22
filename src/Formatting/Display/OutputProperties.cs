namespace SerilogSlim.Formatting.Display;

internal static class OutputProperties
{
    /// <summary>
    /// The message rendered from the log event.
    /// </summary>
    public const string MessagePropertyName = "Message";

    /// <summary>
    /// The timestamp of the log event.
    /// </summary>
    public const string TimestampPropertyName = "Timestamp";

    /// <summary>
    /// The level of the log event.
    /// </summary>
    public const string LevelPropertyName = "Level";

    /// <summary>
    /// A new line.
    /// </summary>
    public const string NewLinePropertyName = "NewLine";

    /// <summary>
    /// The exception associated with the log event.
    /// </summary>
    public const string ExceptionPropertyName = "Exception";

    /// <summary>
    /// The properties of the log event.
    /// </summary>
    public const string PropertiesPropertyName = "Properties";
}