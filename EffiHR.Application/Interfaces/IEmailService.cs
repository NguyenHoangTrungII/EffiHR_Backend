using EffiHR.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EffiHR.Core.DTOs.Auth;

namespace EffiHR.Application.Interfaces
{
    public interface IEmailService
    {
        // Phương thức gửi email
        Task<ApiResponse<bool>> SendMail(EmailContent mailContent);

        // Phương thức gửi email với các tham số đơn giản
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}
