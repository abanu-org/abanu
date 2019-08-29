namespace lonos.kernel.core.Scheduling
{
    public enum ThreadStatus
    {
        Empty = 0,
        Creating,
        Created,
        Running,
        Terminated,
        Waiting
    };
}
