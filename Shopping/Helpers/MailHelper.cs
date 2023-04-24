using MailKit.Net.Smtp;
using MimeKit;
using Shopping.Common;

namespace Shopping.Helpers
{
    public class MailHelper : IMailHelper
    {
        //Esto sirve para leer lo del appsettings.json
        private readonly IConfiguration _configuration;

        //Inyectamos para la leer la configuración
        public MailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Response SendMail(string toName, string toEmail, string subject, string body)
        {
            try
            {
                //Lee los valores de configuracion del appsettings
                string from = _configuration["Mail:From"];
                string name = _configuration["Mail:Name"];
                string smtp = _configuration["Mail:Smtp"];
                string port = _configuration["Mail:Port"];
                string password = _configuration["Mail:Password"];

                //Creacion del nuevo mensaje
                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress(name, from));
                //toName: usuario al que envio toEmail: desde donde le envio
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;
                BodyBuilder bodyBuilder = new ()
                {
                    HtmlBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                using (SmtpClient client = new ())
                {
                    client.Connect(smtp, int.Parse(port), false);
                    client.Authenticate(from, password);
                    client.Send(message);
                    client.Disconnect(true);
                }
                //Si pudo enviar
                return new Response { IsSuccess = true };

            }
            catch (Exception ex)
            {
                return new Response
                {
                    //No envio correo
                    IsSuccess = false,
                    Message = ex.Message,
                    Result = ex
                };
            }

        }
    }
}
