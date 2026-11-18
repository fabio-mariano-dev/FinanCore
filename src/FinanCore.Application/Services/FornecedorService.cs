using FinanCore.Application.Abstractions;
using FinanCore.Application.DTOs.Fornecedores;
using FinanCore.Domain.Entities;
using FinanCore.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FinanCore.Application.Services;

public class FornecedorService(
    IFinanCoreDbContext dbContext,
    IValidator<CriarFornecedorRequest> criarValidator,
    IValidator<AtualizarFornecedorRequest> atualizarValidator) : IFornecedorService
{
    public async Task<FornecedorResponse> CriarAsync(CriarFornecedorRequest request, CancellationToken cancellationToken = default)
    {
        await criarValidator.ValidateAndThrowAsync(request, cancellationToken);
        if (await ExisteDocumentoDuplicadoAsync(request.DocumentoFiscal, null, cancellationToken))
            throw new RegraNegocioException("Já existe um fornecedor com este CPF/CNPJ.");

        var fornecedor = new Fornecedor(request.Nome, request.DocumentoFiscal, request.Email, request.Telefone, request.NomeFantasia, request.InscricaoEstadual, request.InscricaoMunicipal, request.Celular, request.Cep, request.Logradouro, request.Numero, request.Complemento, request.Bairro, request.Cidade, request.Uf);

        dbContext.Fornecedores.Add(fornecedor);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ParaResponse(fornecedor);
    }

    public async Task<FornecedorResponse> AtualizarAsync(Guid id, AtualizarFornecedorRequest request, CancellationToken cancellationToken = default)
    {
        await atualizarValidator.ValidateAndThrowAsync(request, cancellationToken);
        if (await ExisteDocumentoDuplicadoAsync(request.DocumentoFiscal, id, cancellationToken))
            throw new RegraNegocioException("Já existe um fornecedor com este CPF/CNPJ.");

        var fornecedor = await ObterEntidadeAsync(id, cancellationToken);

        fornecedor.Atualizar(request.Nome, request.DocumentoFiscal, request.Email, request.Telefone, request.NomeFantasia, request.InscricaoEstadual, request.InscricaoMunicipal, request.Celular, request.Cep, request.Logradouro, request.Numero, request.Complemento, request.Bairro, request.Cidade, request.Uf);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ParaResponse(fornecedor);
    }

    public async Task<FornecedorResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var fornecedor = await ObterEntidadeAsync(id, cancellationToken);
        return ParaResponse(fornecedor);
    }

    public async Task<IReadOnlyList<FornecedorResponse>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var fornecedores = await dbContext.Fornecedores
            .AsNoTracking()
            .OrderBy(f => f.Nome)
            .ToListAsync(cancellationToken);

        return fornecedores.Select(ParaResponse).ToList();
    }

    public async Task DesativarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var fornecedor = await ObterEntidadeAsync(id, cancellationToken);

        fornecedor.Desativar();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AtivarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var fornecedor = await ObterEntidadeAsync(id, cancellationToken);
        fornecedor.Ativar();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Fornecedor> ObterEntidadeAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Fornecedores.FirstOrDefaultAsync(f => f.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Fornecedor '{id}' não encontrado.");
    }

    private static FornecedorResponse ParaResponse(Fornecedor fornecedor)
        => new(fornecedor.Id, fornecedor.Nome, fornecedor.DocumentoFiscal, fornecedor.Email, fornecedor.Telefone, fornecedor.Ativo, fornecedor.NomeFantasia, fornecedor.InscricaoEstadual, fornecedor.InscricaoMunicipal, fornecedor.Celular, fornecedor.Cep, fornecedor.Logradouro, fornecedor.Numero, fornecedor.Complemento, fornecedor.Bairro, fornecedor.Cidade, fornecedor.Uf);

    private Task<bool> ExisteDocumentoDuplicadoAsync(string? documentoFiscal, Guid? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentoFiscal)) return Task.FromResult(false);
        var documento = documentoFiscal.Trim();
        return dbContext.Fornecedores.AnyAsync(f => f.DocumentoFiscal == documento && (!id.HasValue || f.Id != id.Value), cancellationToken);
    }
}
