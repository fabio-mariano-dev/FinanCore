using FinanCore.Application.DTOs.Categorias;

namespace FinanCore.Application.Abstractions;

public interface ICategoriaService
{
    Task<CategoriaResponse> CriarAsync(CriarCategoriaRequest request, CancellationToken cancellationToken = default);
    Task<CategoriaResponse> AtualizarAsync(Guid id, AtualizarCategoriaRequest request, CancellationToken cancellationToken = default);
    Task<CategoriaResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoriaResponse>> ListarAsync(CancellationToken cancellationToken = default);
    Task DesativarAsync(Guid id, CancellationToken cancellationToken = default);
    Task AtivarAsync(Guid id, CancellationToken cancellationToken = default);
}
