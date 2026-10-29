namespace BankingFraudDetector.Domain.Events;

public enum AccountEventType
{
    PasswordReset = 1,
    MfaEnrolled = 2,
    ContactDetailsChanged = 3,
    AddressChanged = 4,
    BankDetailsChanged = 5,
    SaleRequested = 6,
    TransferRequested = 7
}