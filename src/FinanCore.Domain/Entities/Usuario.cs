using FinanCore.Domain.Common;
using FinanCore.Domain.Exceptions;

namespace FinanCore.Domain.Entities;

public class Usuario : BaseEntity
{
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string SenhaHash { get; private set; }
    public Guid PerfilId { get; private set; }
    public bool Ativo { get; private set; }

    public Usuario(string nome, string email, string senhaHash, Guid perfilId)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException("O nome do usuário é obrigatório.");

        if (string.IsNullOrWhiteSpace(email))
            throw new RegraNegocioException("O e-mail do usuário é obrigatório.");

        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new RegraNegocioException("A senha do usuário é obrigatória.");

        Nome = nome;
        Email = email;
        SenhaHash = senhaHash;
        PerfilId = perfilId;
        Ativo = true;
    }

    public void Desativar() => Ativo = false;
}
