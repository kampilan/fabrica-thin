using Fabrica.Utilities.Queue;

namespace Fabrica.Aws;

public class CompletionHandle: ICompletionHandle
{
    internal string QueueUrl { get; set; } = string.Empty;
    internal string Receipt { get; set; } = string.Empty;
}