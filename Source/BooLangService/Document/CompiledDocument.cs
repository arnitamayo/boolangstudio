using Boo.BooLangService.Document.Nodes;

namespace Boo.BooLangService.Document
{
    public class CompiledDocument
    {
        private readonly IBooParseTreeNode root;
        private readonly string content;

        public CompiledDocument(IBooParseTreeNode root, string content)
        {
            this.root = root;
            this.content = content;
        }

        public string Content
        {
            get { return content; }
        }

        public IBooParseTreeNode GetScopeByLine(int line)
        {
            return GetScopeByLine(root, line);
        }

        private IBooParseTreeNode GetScopeByLine(IBooParseTreeNode node, int line)
        {
            foreach (IBooParseTreeNode child in node.Children)
            {
                IBooParseTreeNode foundNode = GetScopeByLine(child, line);

                if (foundNode != null)
                    return foundNode;
            }

            bool isScopable = AttributeHelper.Has<ScopableAttribute>(node.GetType());

            if (isScopable && node.ContainsLine(line))
                return node;

            return null;
        }
    }
}