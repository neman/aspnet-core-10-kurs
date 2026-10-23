namespace HeroRegistry.Models;

public sealed class Power
{
    private Power() { }

    public Power(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleException("Power name is required.");

        Name = name.Trim();
    }

    public int Id { get; internal set; }
    public string Name { get; private set; } = "";
}
