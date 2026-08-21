namespace Apha.BatchJobs.Worker.Bootstrap;

public static class CliArgumentAdapter
{
    // Bridges positional CLI arg to env var so BatchExecutionRequestResolver picks it up.
    public static void Apply(string[] args)
    {
        if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            Environment.SetEnvironmentVariable("BATCH_JOB_NAME", args[0]);
    }
}
