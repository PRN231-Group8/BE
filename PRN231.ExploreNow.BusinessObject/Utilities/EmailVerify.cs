using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Utilities
{
    public class EmailVerify    
    {
        private readonly IConfiguration _config;

        public EmailVerify(IConfiguration config)
        {
            _config = config;
        }

        public bool SendVerifyAccountEmail(string email, string token)
        {
            var subject = "[ExploreNow] Verify account request";
            var baseUrl = "http://localhost:4200";
            var verifyUrl = $"{baseUrl}/verify-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
            var message = @"
<!DOCTYPE html>
<html dir='ltr' xmlns='http://www.w3.org/1999/xhtml' xmlns:o='urn:schemas-microsoft-com:office:office'>
<head>
  <meta charset='UTF-8'>
  <meta content='width=device-width, initial-scale=1' name='viewport'>
  <meta name='x-apple-disable-message-reformatting'>
  <meta http-equiv='X-UA-Compatible' content='IE=edge'>
  <meta content='telephone=no' name='format-detection'>
  <title></title>
  <style type='text/css'>
    body {
      font-family: Arial, sans-serif;
      background-color: #f2f2f2; /* Light gray background */
      margin: 0;
      padding: 0;
      color: #000000; /* Black text */
    }
    h3 {
      color: #FFC107; /* Yellow headings */
    }
    .verify-btn {
      display: block;
      background-color: #FFC107; /* Yellow button */
      border-radius: 30px;
      font-family: Arial, sans-serif;
      font-size: 22px;
      font-weight: bold;
      line-height: 120%;
      text-align: center;
      text-decoration: none;
      color: #000000; /* Black text */
      padding: 15px 20px;
      margin: 20px 0;
    }
    .email-container {
      width: 600px;
      margin: 0 auto;
      background-color: #f2f2f2; /* Light gray background */
    }
    .email-header {
      background-color: #000000; /* Black header */
      padding: 20px;
      text-align: center;
    }
    .email-body {
      padding: 20px;
      background-color: #f2f2f2; /* Light gray background */
    }
    .email-footer {
      padding: 20px;
      background-color: #f8f9fa;
      text-align: center;
    }
  </style>
</head>
<body>
  <div class='email-container'>
    <div class='email-header'>
      <img src='https://i.pinimg.com/736x/9f/0e/0b/9f0e0b206e1cd6189dadadc0b7736b92.jpg' alt='ExploreNow Logo' width='190' style='display: block; margin: 0 auto;'>
    </div>
    <div class='email-body'>
      <h3>Hi, " + email + @"</h3>
      <p>You're receiving this message because you recently requested to verify your account.</p>
      <p>If this was indeed your request, please click the button below to verify your account:</p>
      <a href='" + verifyUrl + @"' target='_blank' class='verify-btn'>Verify Account</a>
      <p>If you didn't request this, please ignore this email or let us know. This link is valid for 24 hours.</p>
    </div>
    <div class='email-footer'>
      <p>Thanks,<br>ExploreNow Team</p>
    </div>
  </div>
</body>
</html>";

            return SendEmail(email, subject, message);
        }



        private bool SendEmail(string email, string subject, string body)
        {
            {
                var senderEmail = _config["SMTP:Username"];
                var senderPassword = _config["SMTP:Password"];
                var smtpHost = _config["SMTP:Host"];
                var smtpPort = int.Parse(_config["SMTP:Port"]);

                var smtpClient = new SmtpClient(smtpHost)
                {
                    Port = smtpPort,
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);

                smtpClient.Send(mailMessage);
                return true;
            }

        }

    }
}

