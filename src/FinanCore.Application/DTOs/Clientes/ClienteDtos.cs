namespace FinanCore.Application.DTOs.Clientes;

public record CriarClienteRequest(
	string Nome,
	string? DocumentoFiscal,
	string? Email,
	string? Telefone,
	string? NomeFantasia,
	string? InscricaoEstadual,
	string? InscricaoMunicipal,
	string? Celular,
	string? Cep,
	string? Logradouro,
	string? Numero,
	string? Complemento,
	string? Bairro,
	string? Cidade,
	string? Uf);

public record AtualizarClienteRequest(
	string Nome,
	string? DocumentoFiscal,
	string? Email,
	string? Telefone,
	string? NomeFantasia,
	string? InscricaoEstadual,
	string? InscricaoMunicipal,
	string? Celular,
	string? Cep,
	string? Logradouro,
	string? Numero,
	string? Complemento,
	string? Bairro,
	string? Cidade,
	string? Uf);

public record ClienteResponse(
	Guid Id,
	string Nome,
	string? DocumentoFiscal,
	string? Email,
	string? Telefone,
	bool Ativo,
	string? NomeFantasia,
	string? InscricaoEstadual,
	string? InscricaoMunicipal,
	string? Celular,
	string? Cep,
	string? Logradouro,
	string? Numero,
	string? Complemento,
	string? Bairro,
	string? Cidade,
	string? Uf);
