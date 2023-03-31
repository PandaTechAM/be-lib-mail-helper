namespace MailHelper;

public class MailServerConfig
{
    public string SmtpServer { get; set; } = null!;
    public int SmtpPort { get; set; }
    public string SmtpUser { get; set; } = null!;
    public string SmtpPassword { get; set; } = null!;
    public string SmtpFrom { get; set; } = null!;
}