namespace DomainScanner.Application.Pipelines.Interfaces;

public interface INotificationPublisher<T>
{
    T Notification { get; }
}