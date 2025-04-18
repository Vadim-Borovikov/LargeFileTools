using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace Sorter.Config;

internal sealed class Config
{
    [UsedImplicitly]
    [Range(3, ushort.MaxValue)]
    public ushort MaxTempFiles { get; set; }

    [UsedImplicitly]
    [Range(2, int.MaxValue)]
    public int LinesToSortAtOnce { get; set; }

    [UsedImplicitly]
    [Required]
    public string LineFormat { get; set; } = null!;

    [UsedImplicitly]
    [Required]
    public string CustomTempFolderPath { get; set; } = null!;
}