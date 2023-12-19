namespace RobotAppLibraryV2.Modeles.events;

public class RobotEvent<T> : RobotEvent
{
    public RobotEvent(T eventField, string id) : base(id)
    {
        EventField = eventField;
    }

    public T EventField { get; set; }
}

public class RobotEvent : EventArgs
{
    public RobotEvent(string id)
    {
        Id = id;
    }

    public string Id { get; set; }

    public DateTime Date => DateTime.UtcNow;
}