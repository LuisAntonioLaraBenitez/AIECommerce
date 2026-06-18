namespace AIChallenge.Models;

public sealed record RegisterCustomerRequest(
    string FullName,
    string Curp,
    DateOnly BirthDate,
    Address Address);
