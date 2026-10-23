namespace HeroRegistry.Models;

public sealed class Hero
{
    public const int MinOriginStoryLength = 30;

    public static readonly IReadOnlyList<string> ForbiddenPhrases =
        ["tragic past", "chosen one", "mysterious organization"];

    private readonly List<Power> _powers = new();

    private Hero() { }

    public Hero(
        string ownerName,
        string codename,
        string originStory,
        HeroRace race,
        HeroAlignment alignment)
    {
        if (string.IsNullOrWhiteSpace(ownerName))
            throw new BusinessRuleException("A hero needs an owner.");

        OwnerName = ownerName;
        SetDetails(
            codename,
            originStory,
            race,
            alignment);
        Status = HeroStatus.Draft;
    }

    public int Id { get; internal set; }
    public string OwnerName { get; private set; } = null!;
    public string Codename { get; private set; } = "";
    public string OriginStory { get; private set; } = "";
    public HeroRace Race { get; private set; }
    public HeroAlignment Alignment { get; private set; }
    public HeroStatus Status { get; private set; }
    public IReadOnlyList<Power> Powers => _powers.AsReadOnly();

    public void UpdateDraft(
        string codename,
        string originStory,
        HeroRace race,
        HeroAlignment alignment)
    {
        if (Status != HeroStatus.Draft)
            throw new InvalidStatusException(Status, "Only a Draft hero can be edited.");

        SetDetails(
            codename,
            originStory,
            race,
            alignment);
    }

    public void AddPower(Power power)
    {
        ArgumentNullException.ThrowIfNull(power);
        if (Status == HeroStatus.Retired)
            throw new InvalidStatusException(Status, "A Retired hero is read-only.");

        if (!_powers.Contains(power))
            _powers.Add(power);
    }

    // The codename is already guaranteed by the constructor.
    public void Register()
    {
        if (Status != HeroStatus.Draft)
            throw new InvalidStatusException(Status, "Only a Draft hero can be registered.");
        if (OriginStory.Length < MinOriginStoryLength)
            throw new BusinessRuleException(
                $"The origin story needs at least {MinOriginStoryLength} characters.");

        var phrase = ForbiddenPhrases.FirstOrDefault(
            p => OriginStory.Contains(p, StringComparison.OrdinalIgnoreCase));
        if (phrase is not null)
            throw new BusinessRuleException($"The origin story must not contain \"{phrase}\".");

        if (!Enum.IsDefined(Race) || !Enum.IsDefined(Alignment))
            throw new BusinessRuleException("Race and alignment must be valid.");
        if (_powers.Count == 0)
            throw new BusinessRuleException("A hero needs at least one power.");

        Status = HeroStatus.Registered;
    }

    public void Retire()
    {
        if (Status != HeroStatus.Registered)
            throw new InvalidStatusException(Status, "Only a Registered hero can be retired.");

        Status = HeroStatus.Retired;
    }

    public void EnsureCanBeDeleted()
    {
        if (Status != HeroStatus.Draft)
            throw new InvalidStatusException(
                Status,
                "Only a Draft hero can be deleted. Retire a Registered hero instead.");
    }

    private void SetDetails(
        string codename,
        string originStory,
        HeroRace race,
        HeroAlignment alignment)
    {
        if (string.IsNullOrWhiteSpace(codename))
            throw new BusinessRuleException("Codename is required.");

        Codename = codename.Trim();
        OriginStory = originStory?.Trim() ?? "";
        Race = race;
        Alignment = alignment;
    }
}
