namespace SwissWebApplicationFactory.RemoveServices;

[AttributeUsage(AttributeTargets.Class)]
public class RemoveAttribute : Attribute
{
    public RemoveOrder RemoveOrder { get; }

    public RemoveAttribute(RemoveOrder removeOrder = RemoveOrder.All)
    {
        RemoveOrder = removeOrder;
    }
}