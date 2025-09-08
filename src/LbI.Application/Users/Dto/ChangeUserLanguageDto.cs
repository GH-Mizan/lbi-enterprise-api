using System.ComponentModel.DataAnnotations;

namespace LbI.Users.Dto;

public class ChangeUserLanguageDto
{
    [Required]
    public string LanguageName { get; set; }
}