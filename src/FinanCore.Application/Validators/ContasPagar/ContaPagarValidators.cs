using FinanCore.Application.DTOs.ContasPagar;
using FluentValidation;

namespace FinanCore.Application.Validators.ContasPagar;

public class CriarContaPagarRequestValidator : AbstractValidator<CriarContaPagarRequest>
{
    public CriarContaPagarRequestValidator()
    {
        RuleFor(x => x.FornecedorId).NotEmpty();
        RuleFor(x => x.CategoriaId).NotEmpty();
        RuleFor(x => x.Descricao).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Valor).GreaterThan(0);
        RuleFor(x => x.DataVencimento).NotEqual(default(DateTime));
    }
}

public class AtualizarContaPagarRequestValidator : AbstractValidator<AtualizarContaPagarRequest>
{
    public AtualizarContaPagarRequestValidator()
    {
        RuleFor(x => x.FornecedorId).NotEmpty();
        RuleFor(x => x.CategoriaId).NotEmpty();
        RuleFor(x => x.Descricao).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Valor).GreaterThan(0);
        RuleFor(x => x.DataVencimento).NotEqual(default(DateTime));
    }
}

public class RegistrarPagamentoRequestValidator : AbstractValidator<RegistrarPagamentoRequest>
{
    public RegistrarPagamentoRequestValidator()
    {
        RuleFor(x => x.DataPagamento).NotEqual(default(DateTime));
    }
}
