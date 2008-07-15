using Boo.BooLangService.Intellisense;

namespace Boo.BooLangService.Document.Nodes
{
    [Scopable, IntellisenseVisible]
    public class StructTreeNode : TypeDeclarationTreeNode
    {
        public StructTreeNode(string fullName)
            : base(fullName)
        {}
    }
}