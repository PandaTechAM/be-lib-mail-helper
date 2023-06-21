using System.Net;
using System.Net.Mail;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PandaTech.MailHelper;

public class MailSender : BackgroundService
{
    public class Message
    {
        public string To { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;

        [JsonIgnore]
        public MailServerConfig? MailServerConfig { get; set; } = null!;

        public List<AttachmentData> Attachments { get; set; } = new();
    }

    public struct AttachmentData
    {
        public string Name { get; set; }
        public byte[] Data { get; set; }
    }

    private readonly MailServerConfig? _mailServerConfig;
    private readonly Queue<Message> _messageQueue = new();
    private readonly object _lock = new();
    private readonly PeriodicTimer _timer;
    private readonly ILogger<MailSender> _logger;

    public MailSender(MailServerConfig? mailServerConfig, ILogger<MailSender> logger)
    {
        _mailServerConfig = mailServerConfig;
        _logger = logger;
        var time = Environment.GetEnvironmentVariable("MAIL_SENDER_TIMER") ?? "5";
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(int.Parse(time)));
    }

    public void SendMail(string to, string subject, string body, List<AttachmentData>? attachments = null)
    {
        Message message = new()
        {
            To = to,
            Subject = subject,
            Body = body,
            Attachments = attachments ?? new List<AttachmentData>()
        };
        lock (_lock)
        {
            _messageQueue.Enqueue(message);
            _logger.LogInformation("Mail added to queue");
        }
    }

    public List<Message> GetMessages()
    {
        lock (_lock)
        {
            return _messageQueue.ToList();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            int count;
            lock (_lock)
            {
                count = _messageQueue.Count;
            }

            if (count == 0) continue;

            _logger.LogInformation("Mail queue count: {count}", count);


            while (count > 0)
            {
                count--;
                Message messageToSend;
                lock (_lock)
                {
                    messageToSend = _messageQueue.Dequeue();
                }

                var config = (messageToSend.MailServerConfig ?? _mailServerConfig);

                if (config is null)
                    continue;

                var smtpClient = new SmtpClient(config.SmtpServer, config.SmtpPort)
                {
                    Credentials = new NetworkCredential(config.SmtpUser, config.SmtpPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage(config.SmtpFrom,
                    messageToSend.To)
                {
                    Subject = messageToSend.Subject,
                    Body = messageToSend.Body,
                };
                foreach (var mailAttachment in messageToSend.Attachments.Select(attachment =>
                             new Attachment(new MemoryStream(attachment.Data), attachment.Name)))
                {
                    mailMessage.Attachments.Add(mailAttachment);
                }

                try
                {
                    smtpClient.Send(mailMessage);
                    _logger.LogInformation("Mail {subject} sent to {to}", messageToSend.Subject, messageToSend.To);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error sending mail, re-adding to queue");
                    lock (_lock)
                    {
                        _messageQueue.Enqueue(messageToSend);
                    }
                }
            }

            _logger.LogInformation("Mail queue count: {count}", count);
        }
    }
}