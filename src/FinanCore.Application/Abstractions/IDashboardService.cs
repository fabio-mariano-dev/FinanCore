using FinanCore.Application.DTOs.Dashboard;

namespace FinanCore.Application.Abstractions;

public interface IDashboardService
{
    Task<DashboardResponse> ObterAsync(DateTime? periodoInicio = null, DateTime? periodoFim = null, CancellationToken cancellationToken = default);
}
