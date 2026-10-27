namespace SHM.Domain;

public abstract class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public int SiteId { get; set; }
    public DeviceStatus Status { get; set; } = DeviceStatus.Unknown;

    public abstract AlertSeverity Severity { get; }
}