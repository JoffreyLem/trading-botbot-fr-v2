namespace StrategyApi.Mail;

public interface IEmailService
{
    Task SendEmail(string subject, string body);
}