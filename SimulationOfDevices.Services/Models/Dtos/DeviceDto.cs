namespace SimulationOfDevices.Services.Models.Dtos;

public class DeviceDto
{
    public long Id { get; set; }

    public long SemanticId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string Protocol { get; set; } = string.Empty;

    public NamedObject? Type { get; set; }

    public long? LastReport { get; set; }

    public bool UpdatableByUser { get; set; }

    public long DefaultGroupId { get; set; }
}