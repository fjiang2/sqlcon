namespace Sys.Data
{
    public interface IForeignKeys
    {
        IForeignKey[] Keys { get; }
        int Length { get; }
    }
}
