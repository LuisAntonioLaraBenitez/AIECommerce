namespace AIChallenge.Models;

public sealed record OrderStatusDetail(
    string? RejectionReason,
    IReadOnlyList<string> DeliveryTracking);
