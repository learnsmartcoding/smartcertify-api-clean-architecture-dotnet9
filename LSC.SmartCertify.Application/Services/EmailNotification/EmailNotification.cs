using LSC.SmartCertify.Application.DTOs;
using LSC.SmartCertify.Application.Interfaces.EmailNotification;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LSC.SmartCertify.Application.Services.EmailNotification
{
    public class EmailNotification : IEmailNotification
    {
        private readonly IConfiguration configuration;

        public EmailNotification(IConfiguration configuration)
        {
            this.configuration = configuration;
        }


        public async Task<Response> SendPasswordResetEmailForUser(string userEmailId, string tempPassword)
        {
            var apiKey = configuration["SendGrid:SENDGRID_API_KEY"];
            var from = new EmailAddress(configuration["SendGrid:From"]);
            var to = new EmailAddress(userEmailId);

            var sendGridMessage = new SendGridMessage()
            {
                From = from,
                ReplyTo = to,
                Subject = "Your Temporary Password"
            };

            var body = $"Dear user,\n\nYour password has been reset. Please use the following temporary password to log in:\n\n{tempPassword}\n\nYou will be prompted to change it on your next sign-in.\n\nBest regards,\nLearn Smart Coding's Support Team";

            sendGridMessage.AddContent(MimeType.Html, GetEmailContentForPasswordReset(body));
            sendGridMessage.AddTo(to);

            Console.WriteLine($"Sending email with payload: \n{sendGridMessage.Serialize()}");

            var response = await new SendGridClient(apiKey).SendEmailAsync(sendGridMessage).ConfigureAwait(false);
            Console.WriteLine($"Response: {response.StatusCode}");
            Console.WriteLine(response.Headers);

            return response;
        }

        public async Task<Response> SendEmailForContactUs(ContactMessageDto contactMessage)
        {
            var apiKey = configuration["SendGrid:SENDGRID_API_KEY"];
            var from = new EmailAddress(configuration["SendGrid:From"]);
            var to = new EmailAddress(configuration["SendGrid:From"], "Karthik");

            var sendGridMessage = new SendGridMessage()
            {
                From = from,
                ReplyTo = to,
                Subject = "Contact page: Received a request from user"
            };

            sendGridMessage.AddContent(MimeType.Html, GetEmailContent(contactMessage));
            sendGridMessage.AddTo(to);

            Console.WriteLine($"Sending email with payload: \n{sendGridMessage.Serialize()}");

            var response = await new SendGridClient(apiKey).SendEmailAsync(sendGridMessage).ConfigureAwait(false);
            Console.WriteLine($"Response: {response.StatusCode}");
            Console.WriteLine(response.Headers);

            return response;
        }

        private string GetEmailContentForPasswordReset(string body)
        {
            return $$"""
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset=""UTF-8"">
                        <title>An password reset request received  - Your Temporary Password</title>
                    </head>
                    <body>                        
                        <p>{{body}}</p>                  
                    </body>
                    </html>                    
                    """;
        }
        private string GetEmailContent(ContactMessageDto contactMessage)
        {

            return $$"""
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset=""UTF-8"">
                        <title>An enquiry received  - {{contactMessage.Subject}}</title>
                    </head>
                    <body>                        
                        <p>Dear LearnSmartCoding</p>
                        <p>You have received an enquiry from a user and the details as follows.</p>
                    
                        <p><strong>Message details</strong></p>
                        <ul>
                            <li>User Name: {{contactMessage.Name}}</li>
                            <li>User Email: {{contactMessage.Email}}</li>
                    <li>Subject: {{contactMessage.Subject}}</li>
                    <li>Message: {{contactMessage.Message}}</li>
                        </ul>
                    
                    
                        <p><strong>Warm regards,</strong></p>
                        <p>LearnSmartCoding [Automated]</p>
                    </body>
                    </html>                    
                    """;
        }

    }
}
