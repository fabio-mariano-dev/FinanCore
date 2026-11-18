using FinanCore.Application.DTOs.Movimentacoes;

namespace FinanCore.Application.Abstractions;

public interface IMovimentacaoFinanceiraService
{
    Task<IReadOnlyList<MovimentacaoResponse>> ListarAsync(MovimentacaoFiltro? filtro = null, CancellationToken cancellationToken = default);

    Task<FluxoCaixaResponse> ObterFluxoCaixaAsync(DateTime periodoInicio, DateTime periodoFim, CancellationToken cancellationToken = default);
}
