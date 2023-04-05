# PandaTech.MailHelper NuGet

## Introduction

PandaTech.MailHelper is a .NET library that simplifies sending emails using the System.Net.Mail namespace. It provides a simple interface for sending emails asynchronously, with support for attachments.

## Installation

The library can be installed via NuGet Package Manager or by adding a reference to the PandaTech.MailHelper package in your project.

## Usage

### Configuration

The `MailServerConfig` class is used to configure the SMTP server details:

```csharp
public class MailServerConfig
{
    public string SmtpServer { get; set; } = null!;
    public int SmtpPort { get; set; }
    public string SmtpUser { get; set; } = null!;
    public string SmtpPassword { get; set; } = null!;
    public string SmtpFrom { get; set; } = null!;
}
```

To use PandaTech.MailHelper, create an instance of `MailSender` by passing a `MailServerConfig` instance to its constructor:

```csharp
var mailConfig = new MailServerConfig
{
    SmtpServer = "smtp.gmail.com",
    SmtpPort = 587,
    SmtpUser = "your_email@gmail.com",
    SmtpPassword = "your_password",
    SmtpFrom = "your_email@gmail.com",
};

var mailSender = new MailSender(mailConfig, logger);
```

### Sending Emails
```csharp
mailSender.SendMail("recipient@example.com", "Subject", "Body");
```

To include attachments, pass an optional `List<AttachmentData>` parameter to the `SendMail` method:

```csharp
var attachment = new AttachmentData
{
    Name = "attachment.txt",
    Data = File.ReadAllBytes("path/to/attachment.txt"),
};
var attachments = new List<AttachmentData> { attachment };

mailSender.SendMail("recipient@example.com", "Subject", "Body", attachments);
```

### Credits
PandaTech.MailHelper was created by <u>PandaTech Ltd</u> and is licensed under the <u>MIT license</u>.