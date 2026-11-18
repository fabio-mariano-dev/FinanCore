namespace FinanCore.Web.Models;

public enum CategoriaTipo
{
    Receita = 1,
    Despesa = 2
}

public sealed record CategoriaResponse(Guid Id, string Nome, CategoriaTipo Tipo, bool Ativo);
public sealed record CategoriaRequest(string Nome, CategoriaTipo Tipo);
