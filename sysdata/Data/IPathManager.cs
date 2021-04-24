using Sys;
using Sys.Data;
using Sys.Stdio;

namespace sqlcon
{
    public interface IPathManager
    {
        TreeNode<IDataPath> CurrentNode { get; set; }
        IDataPath Current { get; }
        TreeNode<IDataPath> RootNode { get; }

        void Expand(TreeNode<IDataPath> pt, bool refresh);
        Locator GetCombinedLocator(TreeNode<IDataPath> pt1);
        TreeNode<IDataPath> GetCurrentNode<T>() where T : IDataPath;
        T GetCurrentPath<T>() where T : IDataPath;
        TreeNode<IDataPath> GetNodeFrom<T>(TreeNode<IDataPath> current) where T : IDataPath;
        T GetPathFrom<T>(TreeNode<IDataPath> current) where T : IDataPath;
        TreeNode<IDataPath> Navigate(PathName pathName);
    }
}