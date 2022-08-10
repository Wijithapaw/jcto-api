namespace JCTO.Domain.CustomExceptions
{
    public class JCTOException : Exception
    {
        public JCTOException() : base() { }
        public JCTOException(string message) : base(message) { }
        public JCTOException(string message, Exception innerException) : base(message, innerException) { }
    }
}
