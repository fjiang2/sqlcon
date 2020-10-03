namespace Sys.Data.Linq
{
    public interface IAssociation
    {
        bool IsForeignKey { get; set; }
        string Name { get; set; }
        string OtherKey { get; set; }
        string ThisKey { get; set; }
    }
}