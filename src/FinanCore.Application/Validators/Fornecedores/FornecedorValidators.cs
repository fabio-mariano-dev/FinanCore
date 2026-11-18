using FinanCore.Application.DTOs.Fornecedores;
using FluentValidation;

namespace FinanCore.Application.Validators.Fornecedores;

public class CriarFornecedorRequestValidator : AbstractValidator<CriarFornecedorRequest>
{
    public CriarFornecedorRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);

        RuleFor(x => x.DocumentoFiscal)
            .Must(DocumentoFiscalRules.SerDocumentoValido)
            .WithMessage("Documento fiscal inválido: informe um CPF (11 dígitos) ou CNPJ (14 dígitos).");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Uf).Length(2).When(x => !string.IsNullOrWhiteSpace(x.Uf));
    }
}

public class AtualizarFornecedorRequestValidator : AbstractValidator<AtualizarFornecedorRequest>
{
    public AtualizarFornecedorRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);

        RuleFor(x => x.DocumentoFiscal)
            .Must(DocumentoFiscalRules.SerDocumentoValido)
            .WithMessage("Documento fiscal inválido: informe um CPF (11 dígitos) ou CNPJ (14 dígitos).");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Uf).Length(2).When(x => !string.IsNullOrWhiteSpace(x.Uf));
    }
}
