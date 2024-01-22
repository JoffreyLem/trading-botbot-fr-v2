namespace Robot.Mail;

public interface IEmailService
{
    Task SendEmail(string subject, string body);
}