namespace Jiangyi.EventBus;
public class EventBusOption
{
    public Uri Uri { get; set; }
    public string SubscriptionClientName { get; set; }
    public int RetryCount { get; set; }
}