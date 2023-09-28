namespace RobotAppLibraryV2.ApiHandler.Xtb.utils;

internal class ExecuteWithTimeLimit
{
    public static bool Execute(TimeSpan timeSpan, Action codeBlock)
    {
        try
        {
            var task = Task.Factory.StartNew(() => codeBlock());
            task.Wait(timeSpan);
            return task.IsCompleted;
        }
        catch (AggregateException ae)
        {
            throw ae.InnerExceptions[0];
        }
    }
}