namespace Sys.Data
{
    public interface IPrimaryKeys
    {
        string[] Keys { get; }
        int Length { get; }
        string ConstraintName { get; }
    }
}
