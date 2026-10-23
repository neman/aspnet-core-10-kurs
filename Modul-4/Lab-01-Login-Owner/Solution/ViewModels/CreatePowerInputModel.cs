using System.ComponentModel.DataAnnotations;

namespace HeroRegistry.ViewModels;

public sealed class CreatePowerInputModel
{
    [Required, StringLength(40)]
    public string Name { get; set; } = "";
}
