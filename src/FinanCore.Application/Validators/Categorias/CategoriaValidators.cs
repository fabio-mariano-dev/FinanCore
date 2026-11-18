using FinanCore.Application.DTOs.Categorias;
using FluentValidation;

namespace FinanCore.Application.Validators.Categorias;

public class CriarCategoriaRequestValidator : AbstractValidator<CriarCategoriaRequest>
{
    public CriarCategoriaRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Tipo).IsInEnum();
    }
}

public class AtualizarCategoriaRequestValidator : AbstractValidator<AtualizarCategoriaRequest>
{
    public AtualizarCategoriaRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Tipo).IsInEnum();
    }
}
