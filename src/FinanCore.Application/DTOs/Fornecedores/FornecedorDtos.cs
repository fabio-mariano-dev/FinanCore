namespace FinanCore.Application.DTOs.Fornecedores;

public record CriarFornecedorRequest(string Nome, string? DocumentoFiscal, string? Email, string? Telefone, string? NomeFantasia, string? InscricaoEstadual, string? InscricaoMunicipal, string? Celular, string? Cep, string? Logradouro, string? Numero, string? Complemento, string? Bairro, string? Cidade, string? Uf);

public record AtualizarFornecedorRequest(string Nome, string? DocumentoFiscal, string? Email, string? Telefone, string? NomeFantasia, string? InscricaoEstadual, string? InscricaoMunicipal, string? Celular, string? Cep, string? Logradouro, string? Numero, string? Complemento, string? Bairro, string? Cidade, string? Uf);

public record FornecedorResponse(Guid Id, string Nome, string? DocumentoFiscal, string? Email, string? Telefone, bool Ativo, string? NomeFantasia, string? InscricaoEstadual, string? InscricaoMunicipal, string? Celular, string? Cep, string? Logradouro, string? Numero, string? Complemento, string? Bairro, string? Cidade, string? Uf);
