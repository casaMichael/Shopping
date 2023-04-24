namespace Shopping.Common
{
    public class Response
    {
        // IsSuccess si pudo o no ahacer la acción al enviar el correo
        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        //
        public object Result { get; set; }

    }
}
