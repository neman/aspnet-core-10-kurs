namespace HeroRegistry.Models;

// Base for every rule the domain enforces; the web layer can catch it in one place.
public abstract class DomainException(string message) : Exception(message);

// The data does not satisfy a rule (e.g. the origin story is too short).
public sealed class BusinessRuleException(string message) : DomainException(message);

// The operation is not allowed in the hero's current status (e.g. editing a Registered hero).
public sealed class InvalidStatusException(HeroStatus status, string message)
    : DomainException(message)
{
    public HeroStatus Status { get; } = status;
}
