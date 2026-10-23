using System.ComponentModel.DataAnnotations;

namespace HeroRegistry.ViewModels;

public sealed class LoginInputModel
{
    [Required]
    public string Name { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}
