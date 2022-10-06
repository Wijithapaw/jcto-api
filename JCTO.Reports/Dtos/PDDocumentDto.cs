namespace JCTO.Reports.Dtos
{
    public class PDDocumentDto
    {
        public string Customer { get; set; }
        public string Product { get; set; }
        public string Buyer { get; set; }
        public string EntryNo { get; set; }
        public string ObRef { get; set; }
        public List<PDBowserDetails> BowserList { get; set; }
    }

    public class PDBowserDetails
    {
        public int Capacity { get; set; }
        public int Count { get; set; }
    }
}
