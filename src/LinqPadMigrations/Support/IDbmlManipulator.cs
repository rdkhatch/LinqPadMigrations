
namespace LinqPadMigrations.Support
{
    public interface IDbmlManipulator
    {
        string ManipulateDBML(string dbmlXmlContents);
    }
}
