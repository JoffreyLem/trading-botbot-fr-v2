namespace Robot.Server.Events;

public class BackGroundServiceEvent<T> : DtoEventArgsBase
{
    public BackGroundServiceEvent(T eventField, string id) : base(id)
    {
        EventField = eventField;
    }

    public T EventField { get; set; }
}

public class DtoEventArgsBase : EventArgs
{
    protected DtoEventArgsBase(string id)
    {
        Id = id;
    }

    public string Id { get; set; }
}