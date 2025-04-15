using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace Generator;

internal sealed class Config
{
    [UsedImplicitly]
    [Required]
    public string PoolFilePath { get; set; } = null!;

    [UsedImplicitly]
    [Range(1, ushort.MaxValue)]
    public ushort MemoryUsageMegaBytesPerWorker { get; set; }

    [UsedImplicitly]
    [Required]
    public string LineFormat { get; set; } = null!;
}