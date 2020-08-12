namespace Jerrycurl.Data.Metadata
{
    public enum ReferenceKeyType
    {

        CandidateKey = 0,
        ForeignKey = 1,
    }

    public enum ReferenceKeyFlags
    {
        CandidateKey = 1,
        PrimaryKey = CandidateKey | 2,
        ForeignKey = 4,
    }
}