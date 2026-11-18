using FinanCore.Domain.Common;
using FinanCore.Domain.Exceptions;

namespace FinanCore.Domain.Entities;

public class Auditoria : BaseEntity
{
    public Guid UsuarioId { get; private set; }
    public string Entidade { get; private set; }
    public Guid EntidadeId { get; private set; }
    public string Acao { get; private set; }
    public string? DadosAntes { get; private set; }
    public string? DadosDepois { get; private set; }
    public DateTime DataHora { get; private set; }

    public Auditoria(
        Guid usuarioId,
        string entidade,
        Guid entidadeId,
        string acao,
        DateTime dataHora,
        string? dadosAntes = null,
        string? dadosDepois = null)
    {
        if (string.IsNullOrWhiteSpace(entidade))
            throw new RegraNegocioException("A entidade auditada é obrigatória.");

        if (string.IsNullOrWhiteSpace(acao))
            throw new RegraNegocioException("A ação auditada é obrigatória.");

        UsuarioId = usuarioId;
        Entidade = entidade;
        EntidadeId = entidadeId;
        Acao = acao;
        DataHora = dataHora;
        DadosAntes = dadosAntes;
        DadosDepois = dadosDepois;
    }
}
