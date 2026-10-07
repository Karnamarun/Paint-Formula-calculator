namespace PaintTintCalculator.Domain.Exceptions;

public sealed class InvalidCanSizeException : DomainException
{
    public decimal CanSizeLitres { get; }

    public InvalidCanSizeException(decimal canSizeLitres, string supportedSizes)
        : base($"Can size {canSizeLitres}L is not supported. Supported can sizes are: {supportedSizes}.", "INVALID_CAN_SIZE")
    {
        CanSizeLitres = canSizeLitres;
    }
}

