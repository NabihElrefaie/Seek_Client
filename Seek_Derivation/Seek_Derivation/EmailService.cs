using Seek_Derivation;
using System.Net;
using System.Net.Mail;
using System.Text;

public static class EmailService
{
    public static void SendTestEmail(EmailSettings settings)
    {
        try
        {
            using var smtp = CreateSmtpClient(settings);

            using var mail = new MailMessage
            {
                From = new MailAddress(settings.FromEmail, settings.DisplayName),
                Subject = $"{settings.DisplayName} - Email Configuration",
                Body = CreateEmailBody(settings),
                IsBodyHtml = true
            };
            mail.To.Add(settings.AdminEmail);

            smtp.Send(mail);
            Console.WriteLine("\n[✓] Test email sent successfully!");
        }
        catch (SmtpException ex)
        {
            HandleSmtpException(ex);
        }
    }

    public static void SendPasswordToAdmin(EmailSettings settings, string masterPassword)
    {
        try
        {
            using var smtp = CreateSmtpClient(settings);

            var htmlBody = @"
<!DOCTYPE HTML PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional //EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
<!--[if gte mso 9]>
<xml>
  <o:OfficeDocumentSettings>
    <o:AllowPNG/>
    <o:PixelsPerInch>96</o:PixelsPerInch>
  </o:OfficeDocumentSettings>
</xml>
<![endif]-->
  <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <meta name=""x-apple-disable-message-reformatting"">
  <!--[if !mso]><!--><meta http-equiv=""X-UA-Compatible"" content=""IE=edge""><!--<![endif]-->
  <title>Master Password Notification</title>
  
  <style type=""text/css"">
      @media only screen and (min-width: 620px) {
        .u-row {
          width: 600px !important;
        }
        .u-row .u-col {
          vertical-align: top;
        }
        .u-row .u-col-100 {
          width: 600px !important;
        }
      }
      @media only screen and (max-width: 620px) {
        .u-row-container {
          max-width: 100% !important;
          padding-left: 0px !important;
          padding-right: 0px !important;
        }
        .u-row {
          width: 100% !important;
        }
        .u-row .u-col {
          display: block !important;
          width: 100% !important;
          min-width: 320px !important;
          max-width: 100% !important;
        }
        .u-row .u-col > div {
          margin: 0 auto;
        }
        .u-row .u-col img {
          max-width: 100% !important;
        }
      }
      body {
        margin: 0; padding: 0; -webkit-text-size-adjust: 100%; background-color: #e7e7e7; color: #000000; font-family: Arial, Helvetica, sans-serif;
      }
      .badge {
        display: inline-block;
        background-color: #6c757d; /* Secondary color */
        color: white;
        padding: 12px 25px;
        border-radius: 25px;
        font-weight: 700;
        font-size: 20px;
        text-align: center;
        margin-bottom: 20px;
      }
      pre {
        background-color: #f3f4f6;
        padding: 15px 20px;
        border-radius: 6px;
        font-size: 16px;
        line-height: 1.4;
        white-space: pre-wrap;
        word-break: break-word;
        color: #333;
      }
      .footer-text {
        font-size: 14px;
        color: #5b5b5b;
        line-height: 160%;
        text-align: center;
        word-wrap: break-word;
      }
      a {
        color: #0000ee;
        text-decoration: underline;
      }
  </style>
</head>

<body>
  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #e7e7e7; padding: 30px 0;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background-color: #ffffff; border-radius: 8px; overflow: hidden;"">
          <!-- Header Image -->
          <tr>
            <td align=""center"" style=""padding: 60px 20px 10px;"">
              <img src=""https://img.freepik.com/free-vector/illustration-people-avatar-success-business-concept_53876-8764.jpg?semt=ais_hybrid&w=740&q=80"" width=""250"" alt=""Key to Success"" style=""display: block; border: none; outline: none; text-decoration: none; max-width: 100%; height: auto;"" />
            </td>
          </tr>

          <!-- Badge -->
          <tr>
            <td align=""center"" style=""padding: 0 20px 10px;"">
              <div class=""badge"">Master Password</div>
            </td>
          </tr>

          <!-- Heading -->
          <tr>
            <td align=""center"" style=""padding: 0 40px 20px; font-size: 24px; font-weight: 600; color: #333;"">
              Your Master Password Information
            </td>
          </tr>

          <!-- Divider -->
          <tr>
            <td align=""center"">
              <hr style=""border: 0; border-top: 3px solid #0202aa; width: 20%; margin: 0 auto 30px;"" />
            </td>
          </tr>

          <!-- Body Text -->
          <tr>
            <td align=""center"" style=""padding: 0 40px 30px; font-size: 16px; color: #616161; line-height: 1.5;"">
              <p style=""margin: 0 0 10px;"">Here is your master password. Please keep it safe and confidential:</p>
              <pre>" + System.Net.WebUtility.HtmlEncode(masterPassword) + @"</pre>
              <p style=""margin: 20px 0 0; font-weight: 700;"">Keep it secret. Keep it safe.</p>
            </td>
          </tr>

          <!-- Footer Divider -->
          <tr>
            <td>
              <hr style=""border: 0; border-top: 1px solid #bbbbbb; margin: 0 40px 20px;"" />
            </td>
          </tr>

          <!-- Footer Text -->
          <tr>
            <td align=""center"" style=""padding: 0 20px 40px;"">
              <div class=""footer-text"">
                <p>You have received this email as a registered Admin For Seek Application</a></p>
                <p>2361 Elsadat Street #4567 El-Dakhailia, EGY  &bull; All rights reserved</p>
                <p style=""margin-top: 20px;"">Best wishes,<br/>" + settings.DisplayName + @" Team</p>
              </div>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

            using var mail = new MailMessage
            {
                From = new MailAddress(settings.FromEmail, settings.DisplayName),
                Subject = $"{settings.DisplayName} - Master Password",
                Body = htmlBody,
                IsBodyHtml = true
            };
            mail.To.Add(settings.AdminEmail);

            smtp.Send(mail);
            Console.WriteLine("\n[✓] Master password email sent to admin!");
        }
        catch (SmtpException ex)
        {
            HandleSmtpException(ex);
        }
    }


    private static SmtpClient CreateSmtpClient(EmailSettings settings)
    {
        return new SmtpClient(settings.SmtpServer)
        {
            Port = settings.SmtpPort,
            Credentials = new NetworkCredential(settings.Username, settings.Password),
            EnableSsl = settings.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 10000
        };
    }

    private static void HandleSmtpException(SmtpException ex)
    {
        Console.WriteLine($"\nSMTP Error: {ex.StatusCode}");
        Console.WriteLine($"Message: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Details: {ex.InnerException.Message}");
        }
        Console.WriteLine("\nTroubleshooting Tips:");
        Console.WriteLine("1. Verify username/password are correct");
        Console.WriteLine("2. Ensure 'Less Secure Apps' is enabled (for Gmail)");
        Console.WriteLine("3. Try using an App Password if 2FA is enabled");
        Console.WriteLine("4. Check firewall/antivirus settings");
    }

    private static string CreateEmailBody(EmailSettings settings)
    {
        return $@"
<html>
<head>
<style>
    body {{
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        background-color: #f4f6f8;
        color: #333;
        margin: 0;
        padding: 0;
    }}
    .container {{
        background-color: #ffffff;
        max-width: 600px;
        margin: 40px auto;
        padding: 30px;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    }}
    h2 {{
        color: #2c3e50;
        border-bottom: 2px solid #2980b9;
        padding-bottom: 8px;
        margin-bottom: 20px;
    }}
    ul {{
        list-style-type: none;
        padding: 0;
    }}
    li {{
        padding: 10px 0;
        font-size: 16px;
        border-bottom: 1px solid #ecf0f1;
    }}
    li strong {{
        color: #2980b9;
        width: 100px;
        display: inline-block;
    }}
    p {{
        font-size: 14px;
        color: #555;
        margin-top: 30px;
        line-height: 1.5;
    }}
    .footer {{
        text-align: center;
        font-size: 13px;
        color: #999;
        margin-top: 40px;
    }}
    .center {{
        text-align: center;
        font-size: 8px;
        color: #999;
        margin-top: 40px;
    }}
</style>
</head>
<body>
    <div class='container'>
        <h2>SMTP Configuration</h2>
        <ul>
            <li><strong>Server:</strong> {settings.SmtpServer}:{settings.SmtpPort}</li>
            <li><strong>SSL:</strong> {(settings.UseSsl ? "Enabled" : "Disabled")}</li>
            <li><strong>From:</strong> {settings.FromEmail}</li>
        </ul>
        <p class='center'>This email confirms that your SMTP settings are configured correctly and are working properly.</p>
        <div class='footer'>
            <p>© {DateTime.Now.Year} {settings.DisplayName}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }
}
