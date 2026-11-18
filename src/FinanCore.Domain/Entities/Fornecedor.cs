using FinanCore.Domain.Common;
using FinanCore.Domain.Exceptions;

namespace FinanCore.Domain.Entities;

public class Fornecedor : BaseEntity
{
    public string Nome { get; private set; }
    public string? NomeFantasia { get; private set; }
    public string? DocumentoFiscal { get; private set; }
    public string? InscricaoEstadual { get; private set; }
    public string? InscricaoMunicipal { get; private set; }
    public string? Email { get; private set; }
    public string? Telefone { get; private set; }
    public string? Celular { get; private set; }
    public string? Cep { get; private set; }
    public string? Logradouro { get; private set; }
    public string? Numero { get; private set; }
    public string? Complemento { get; private set; }
    public string? Bairro { get; private set; }
    public string? Cidade { get; private set; }
    public string? Uf { get; private set; }
    public bool Ativo { get; private set; }

    public Fornecedor(string nome, string? documentoFiscal = null, string? email = null, string? telefone = null, string? nomeFantasia = null, string? inscricaoEstadual = null, string? inscricaoMunicipal = null, string? celular = null, string? cep = null, string? logradouro = null, string? numero = null, string? complemento = null, string? bairro = null, string? cidade = null, string? uf = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException("O nome do fornecedor é obrigatório.");

        Nome = nome;
        NomeFantasia = nomeFantasia;
        DocumentoFiscal = documentoFiscal;
        InscricaoEstadual = inscricaoEstadual;
        InscricaoMunicipal = inscricaoMunicipal;
        Email = email;
        Telefone = telefone;
        Celular = celular;
        Cep = cep;
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cidade = cidade;
        Uf = uf;
        Ativo = true;
    }

    public void Atualizar(string nome, string? documentoFiscal, string? email, string? telefone, string? nomeFantasia, string? inscricaoEstadual, string? inscricaoMunicipal, string? celular, string? cep, string? logradouro, string? numero, string? complemento, string? bairro, string? cidade, string? uf)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException("O nome do fornecedor é obrigatório.");

        Nome = nome;
        NomeFantasia = nomeFantasia;
        DocumentoFiscal = documentoFiscal;
        InscricaoEstadual = inscricaoEstadual;
        InscricaoMunicipal = inscricaoMunicipal;
        Email = email;
        Telefone = telefone;
        Celular = celular;
        Cep = cep;
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cidade = cidade;
        Uf = uf;
    }

    public void Desativar() => Ativo = false;
    public void Ativar() => Ativo = true;
}
