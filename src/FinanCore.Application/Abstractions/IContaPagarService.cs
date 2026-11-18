using FinanCore.Application.DTOs.ContasPagar;

namespace FinanCore.Application.Abstractions;

public interface IContaPagarService
{
    Task<ContaPagarResponse> CriarAsync(CriarContaPagarRequest request, CancellationToken cancellationToken = default);
    Task<ContaPagarResponse> AtualizarAsync(Guid id, AtualizarContaPagarRequest request, CancellationToken cancellationToken = default);
    Task<ContaPagarResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContaPagarResponse>> ListarAsync(ContaPagarFiltro? filtro = null, CancellationToken cancellationToken = default);
    Task<ContaPagarResponse> RegistrarPagamentoAsync(Guid id, RegistrarPagamentoRequest request, CancellationToken cancellationToken = default);
    Task CancelarAsync(Guid id, CancellationToken cancellationToken = default);
}
