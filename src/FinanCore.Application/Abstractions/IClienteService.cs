using FinanCore.Application.DTOs.Clientes;

namespace FinanCore.Application.Abstractions;

public interface IClienteService
{
    Task<ClienteResponse> CriarAsync(CriarClienteRequest request, CancellationToken cancellationToken = default);
    Task<ClienteResponse> AtualizarAsync(Guid id, AtualizarClienteRequest request, CancellationToken cancellationToken = default);
    Task<ClienteResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClienteResponse>> ListarAsync(CancellationToken cancellationToken = default);
    Task DesativarAsync(Guid id, CancellationToken cancellationToken = default);
    Task AtivarAsync(Guid id, CancellationToken cancellationToken = default);
}
