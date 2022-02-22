using System.ComponentModel.DataAnnotations;

namespace Conference.Data;

public sealed class CreateLecture
{
    [Required]
    public DateTime When { get; set; }

    [Required]
    public int? SpeakerId { get; set; }

    [Required]
    public int? PaperId { get; set; }

    public int SessionId { get; set; }
}