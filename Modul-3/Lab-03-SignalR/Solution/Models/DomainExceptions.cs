namespace HeroRegistry.Models;

public abstract class DomainException(string message) : Exception(message);

public sealed class BusinessRuleException(string message) : DomainException(message);

public sealed class InvalidStatusException(HeroStatus status, string message)
    : DomainException(message)
{
    public HeroStatus Status { get; } = status;
}
