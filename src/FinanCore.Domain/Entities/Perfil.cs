using FinanCore.Domain.Common;
using FinanCore.Domain.Exceptions;

namespace FinanCore.Domain.Entities;

public class Perfil : BaseEntity
{
    public string Nome { get; private set; }

    public Perfil(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException("O nome do perfil é obrigatório.");

        Nome = nome;
    }
}
