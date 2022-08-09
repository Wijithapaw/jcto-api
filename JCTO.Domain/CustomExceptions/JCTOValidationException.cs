namespace JCTO.Domain.CustomExceptions
{
    public class JCTOValidationException : JCTOException
    {
        public string ErrorMesage { get; private set; }

        public JCTOValidationException(string message) : base(message) 
        {
            ErrorMesage = message;
        }
    }
}
