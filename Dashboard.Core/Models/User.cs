namespace BrewHub.Dashboard.Core.Models;

public record User
{
    public string Name { get; init; } = string.Empty;
    public string Sms { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}