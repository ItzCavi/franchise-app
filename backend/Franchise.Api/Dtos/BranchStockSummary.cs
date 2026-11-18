namespace FranchiseApi.Dtos;

public record BranchStockSummary(
    int BranchId,
    string BranchName,
    int ProductCount,
    int TotalStock,
    string? TopProductName,
    int? TopProductStock);