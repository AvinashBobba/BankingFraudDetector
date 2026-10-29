using BankingFraudDetector.Domain.Events;

namespace BankingFraudDetector.Domain.FraudDetection;

public interface IFraudRule
{
    RuleMatch? Evaluate(
        Guid accountId,
        IEnumerable<AccountEvent> events);
}