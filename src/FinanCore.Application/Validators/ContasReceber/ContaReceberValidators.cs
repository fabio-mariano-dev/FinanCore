using FinanCore.Application.DTOs.ContasReceber;
using FluentValidation;

namespace FinanCore.Application.Validators.ContasReceber;

public class CriarContaReceberRequestValidator : AbstractValidator<CriarContaReceberRequest>
{
    public CriarContaReceberRequestValidator()
    {
        RuleFor(x => x.ClienteId).NotEmpty();
        RuleFor(x => x.CategoriaId).NotEmpty();
        RuleFor(x => x.Descricao).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Valor).GreaterThan(0);
        RuleFor(x => x.DataVencimento).NotEqual(default(DateTime));
    }
}

public class AtualizarContaReceberRequestValidator : AbstractValidator<AtualizarContaReceberRequest>
{
    public AtualizarContaReceberRequestValidator()
    {
        RuleFor(x => x.ClienteId).NotEmpty();
        RuleFor(x => x.CategoriaId).NotEmpty();
        RuleFor(x => x.Descricao).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Valor).GreaterThan(0);
        RuleFor(x => x.DataVencimento).NotEqual(default(DateTime));
    }
}

public class RegistrarRecebimentoRequestValidator : AbstractValidator<RegistrarRecebimentoRequest>
{
    public RegistrarRecebimentoRequestValidator()
    {
        RuleFor(x => x.DataRecebimento).NotEqual(default(DateTime));
    }
}
