namespace JCTO.Domain.CustomExceptions
{
    public class JCTOConcurrencyException : JCTOException
    {
        public string Entity { get; private set; }
        public JCTOConcurrencyException(string entity) : base($"Concurrency violation of Entity: {entity.Split('.').Last()}") 
        {
            Entity = entity;
        }
    }
}
