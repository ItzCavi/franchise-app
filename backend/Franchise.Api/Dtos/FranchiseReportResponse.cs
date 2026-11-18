namespace FranchiseApi.Dtos;

public record FranchiseReportResponse(
    int FranchiseId,
    string FranchiseName,
    int BranchCount,
    int ProductCount,
    int TotalStock,
    IReadOnlyList<BranchStockSummary> Branches);