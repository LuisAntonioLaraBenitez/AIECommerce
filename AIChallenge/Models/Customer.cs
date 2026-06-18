namespace AIChallenge.Models;

public sealed record Customer(
    string Id,
    string FullName,
    string Curp,
    DateOnly BirthDate,
    Address Address,
    DateTimeOffset CreatedAt);
