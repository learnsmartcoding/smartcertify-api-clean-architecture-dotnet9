using LSC.SmartCertify.Application.DTOs;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSC.SmartCertify.Application.Interfaces.EmailNotification
{
    public interface IEmailNotification
    {
        Task<Response> SendEmailForContactUs(ContactMessageDto contactMessage);
        Task<Response> SendPasswordResetEmailForUser(string userEmailId, string tempPassword);
    }
}
