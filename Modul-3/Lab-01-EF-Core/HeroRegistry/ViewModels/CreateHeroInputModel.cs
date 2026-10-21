using System.ComponentModel.DataAnnotations;

namespace HeroRegistry.ViewModels;

// Input model for the form: no Id, no Status, so they cannot be posted (overposting).
public sealed class CreateHeroInputModel
{
    [Required, StringLength(60)]
    public string Codename { get; set; } = "";

    [StringLength(1000)]
    [Display(Name = "Origin story")]
    public string? OriginStory { get; set; }

    // Nullable, so that "nothing selected" is null and [Required] can report it.
    [Required]
    public HeroRace? Race { get; set; }

    [Required]
    public HeroAlignment? Alignment { get; set; }
}
