namespace HeroRegistry.Models;

public enum HeroRace
{
    Human = 1,
    Alien = 2,
    Mutant = 3,
    Asgardian = 4
}

public enum HeroAlignment
{
    Heroic = 1,
    Neutral = 2,
    Villainous = 3
}

public enum HeroStatus
{
    Draft,
    Registered,
    Retired
}

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

public sealed class Hero
{
    public const int MinOriginStoryLength = 30;

    private readonly List<Power> _powers = new();

    private Hero() { }

    public Hero(string codename, string originStory,
        HeroRace race, HeroAlignment alignment)
    {
        SetDetails(codename, originStory, race, alignment);
        Status = HeroStatus.Draft;
    }

    public int Id { get; internal set; }
    public string Codename { get; private set; } = "";
    public string OriginStory { get; private set; } = "";
    public HeroRace Race { get; private set; }
    public HeroAlignment Alignment { get; private set; }
    public HeroStatus Status { get; private set; }
    public IReadOnlyList<Power> Powers => _powers;

    public void UpdateDraft(string codename, string originStory,
        HeroRace race, HeroAlignment alignment)
    {
        if (Status != HeroStatus.Draft)
            throw new InvalidStatusException(Status, "Only a Draft hero can be edited.");

        SetDetails(codename, originStory, race, alignment);
    }

    public void AddPower(Power power)
    {
        ArgumentNullException.ThrowIfNull(power);
        if (Status == HeroStatus.Retired)
            throw new InvalidStatusException(Status, "A Retired hero is read-only.");

        if (!_powers.Contains(power))
            _powers.Add(power);
    }

    // Draft -> Registered. The codename is already guaranteed by the constructor.
    public void Register()
    {
        if (Status != HeroStatus.Draft)
            throw new InvalidStatusException(Status, "Only a Draft hero can be registered.");
        if (OriginStory.Length < MinOriginStoryLength)
            throw new BusinessRuleException(
                $"The origin story needs at least {MinOriginStoryLength} characters.");
        if (!Enum.IsDefined(Race) || !Enum.IsDefined(Alignment))
            throw new BusinessRuleException("Race and alignment must be valid.");
        if (_powers.Count == 0)
            throw new BusinessRuleException("A hero needs at least one power.");

        Status = HeroStatus.Registered;
    }

    private void SetDetails(string codename, string originStory,
        HeroRace race, HeroAlignment alignment)
    {
        if (string.IsNullOrWhiteSpace(codename))
            throw new BusinessRuleException("Codename is required.");

        Codename = codename.Trim();
        OriginStory = originStory?.Trim() ?? "";
        Race = race;
        Alignment = alignment;
    }
}
