using FinanCore.Application.DTOs.ContasReceber;

namespace FinanCore.Application.Abstractions;

public interface IContaReceberService
{
    Task<ContaReceberResponse> CriarAsync(CriarContaReceberRequest request, CancellationToken cancellationToken = default);
    Task<ContaReceberResponse> AtualizarAsync(Guid id, AtualizarContaReceberRequest request, CancellationToken cancellationToken = default);
    Task<ContaReceberResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContaReceberResponse>> ListarAsync(ContaReceberFiltro? filtro = null, CancellationToken cancellationToken = default);
    Task<ContaReceberResponse> RegistrarRecebimentoAsync(Guid id, RegistrarRecebimentoRequest request, CancellationToken cancellationToken = default);
    Task CancelarAsync(Guid id, CancellationToken cancellationToken = default);
}
