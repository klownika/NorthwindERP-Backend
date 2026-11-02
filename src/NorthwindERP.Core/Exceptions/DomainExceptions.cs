namespace NorthwindERP.Core.Exceptions;

// Excepciones propias del negocio. El ErrorHandlingMiddleware (capa API)
// las convierte en respuestas HTTP: NotFound -> 404, BusinessRule -> 400.

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
