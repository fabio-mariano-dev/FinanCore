using FinanCore.Domain.Common;
using FinanCore.Domain.Enums;
using FinanCore.Domain.Exceptions;

namespace FinanCore.Domain.Entities;

public class Categoria : BaseEntity
{
    public string Nome { get; private set; }
    public TipoCategoria Tipo { get; private set; }
    public bool Ativo { get; private set; }

    public Categoria(string nome, TipoCategoria tipo)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException("O nome da categoria é obrigatório.");

        Nome = nome;
        Tipo = tipo;
        Ativo = true;
    }

    public void Atualizar(string nome, TipoCategoria tipo)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException("O nome da categoria é obrigatório.");

        Nome = nome;
        Tipo = tipo;
    }

    public void Desativar() => Ativo = false;

    public void Ativar() => Ativo = true;
}
