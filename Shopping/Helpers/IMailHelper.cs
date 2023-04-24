using Shopping.Common;

namespace Shopping.Helpers
{
    public interface IMailHelper
    {
        //Metodo que envia correos (nombre al que enviamos, titulo de correo y cuerpo del correo)
        Response SendMail(string toName, string toEmail, string subject, string body);
    }
}
