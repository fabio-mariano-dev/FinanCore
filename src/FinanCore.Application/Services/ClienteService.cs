using FinanCore.Application.Abstractions;
using FinanCore.Application.DTOs.Clientes;
using FinanCore.Domain.Entities;
using FinanCore.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FinanCore.Application.Services;

public class ClienteService(
    IFinanCoreDbContext dbContext,
    IValidator<CriarClienteRequest> criarValidator,
    IValidator<AtualizarClienteRequest> atualizarValidator) : IClienteService
{
    public async Task<ClienteResponse> CriarAsync(CriarClienteRequest request, CancellationToken cancellationToken = default)
    {
        await criarValidator.ValidateAndThrowAsync(request, cancellationToken);

        if (await ExisteDocumentoDuplicadoAsync(request.DocumentoFiscal, null, cancellationToken))
            throw new RegraNegocioException("Já existe um cliente com este CPF/CNPJ.");

        var cliente = new Cliente(request.Nome, request.DocumentoFiscal, request.Email, request.Telefone, request.NomeFantasia, request.InscricaoEstadual, request.InscricaoMunicipal, request.Celular, request.Cep, request.Logradouro, request.Numero, request.Complemento, request.Bairro, request.Cidade, request.Uf);

        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ParaResponse(cliente);
    }

    public async Task<ClienteResponse> AtualizarAsync(Guid id, AtualizarClienteRequest request, CancellationToken cancellationToken = default)
    {
        await atualizarValidator.ValidateAndThrowAsync(request, cancellationToken);

        if (await ExisteDocumentoDuplicadoAsync(request.DocumentoFiscal, id, cancellationToken))
            throw new RegraNegocioException("Já existe um cliente com este CPF/CNPJ.");

        var cliente = await ObterEntidadeAsync(id, cancellationToken);

        cliente.Atualizar(request.Nome, request.DocumentoFiscal, request.Email, request.Telefone, request.NomeFantasia, request.InscricaoEstadual, request.InscricaoMunicipal, request.Celular, request.Cep, request.Logradouro, request.Numero, request.Complemento, request.Bairro, request.Cidade, request.Uf);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ParaResponse(cliente);
    }

    public async Task<ClienteResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await ObterEntidadeAsync(id, cancellationToken);
        return ParaResponse(cliente);
    }

    public async Task<IReadOnlyList<ClienteResponse>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var clientes = await dbContext.Clientes
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync(cancellationToken);

        return clientes.Select(ParaResponse).ToList();
    }

    public async Task DesativarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await ObterEntidadeAsync(id, cancellationToken);

        cliente.Desativar();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AtivarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await ObterEntidadeAsync(id, cancellationToken);

        cliente.Ativar();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Cliente> ObterEntidadeAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Clientes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Cliente '{id}' não encontrado.");
    }

    private static ClienteResponse ParaResponse(Cliente cliente)
        => new(cliente.Id, cliente.Nome, cliente.DocumentoFiscal, cliente.Email, cliente.Telefone, cliente.Ativo, cliente.NomeFantasia, cliente.InscricaoEstadual, cliente.InscricaoMunicipal, cliente.Celular, cliente.Cep, cliente.Logradouro, cliente.Numero, cliente.Complemento, cliente.Bairro, cliente.Cidade, cliente.Uf);

    private Task<bool> ExisteDocumentoDuplicadoAsync(string? documentoFiscal, Guid? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentoFiscal))
            return Task.FromResult(false);

        var documento = documentoFiscal.Trim();
        return dbContext.Clientes.AnyAsync(c => c.DocumentoFiscal == documento && (!id.HasValue || c.Id != id.Value), cancellationToken);
    }
}
