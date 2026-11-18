using System.Text.RegularExpressions;

namespace FinanCore.Application.Validators;

public static partial class DocumentoFiscalRules
{
    public static bool SerDocumentoValido(string? documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
            return true;

        var apenasDigitos = SomenteDigitos().Replace(documento, string.Empty);

        return apenasDigitos.Length is 11 or 14;
    }

    [GeneratedRegex(@"\D")]
    private static partial Regex SomenteDigitos();
}
