namespace AIChallenge.Models;

public sealed record Address(
    string StreetAndNumber,
    string Neighborhood,
    string PostalCode,
    string Municipality,
    string State);
