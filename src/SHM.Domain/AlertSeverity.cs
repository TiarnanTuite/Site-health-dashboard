namespace SHM.Domain;

public enum AlertSeverity
{
    Low = 0, // one till offline
    Medium = 1, //BOPC online but not receiving sales data
    Critical = 2 // BOPC is down
}