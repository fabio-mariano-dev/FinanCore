using FinanCore.Domain.Enums;

namespace FinanCore.Application.DTOs.Categorias;

public record CriarCategoriaRequest(string Nome, TipoCategoria Tipo);

public record AtualizarCategoriaRequest(string Nome, TipoCategoria Tipo);

public record CategoriaResponse(Guid Id, string Nome, TipoCategoria Tipo, bool Ativo);
