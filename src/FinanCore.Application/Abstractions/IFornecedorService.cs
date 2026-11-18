using FinanCore.Application.DTOs.Fornecedores;

namespace FinanCore.Application.Abstractions;

public interface IFornecedorService
{
    Task<FornecedorResponse> CriarAsync(CriarFornecedorRequest request, CancellationToken cancellationToken = default);
    Task<FornecedorResponse> AtualizarAsync(Guid id, AtualizarFornecedorRequest request, CancellationToken cancellationToken = default);
    Task<FornecedorResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FornecedorResponse>> ListarAsync(CancellationToken cancellationToken = default);
    Task DesativarAsync(Guid id, CancellationToken cancellationToken = default);
    Task AtivarAsync(Guid id, CancellationToken cancellationToken = default);
}
