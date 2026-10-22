using System.ComponentModel.DataAnnotations;

namespace HeroRegistry.ViewModels;

// No Id and no Status, so a form cannot post them (overposting).
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
