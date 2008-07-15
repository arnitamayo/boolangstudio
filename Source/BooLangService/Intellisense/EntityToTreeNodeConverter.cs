using Boo.BooLangService.Document.Nodes;
using Boo.Lang.Compiler.TypeSystem;

namespace Boo.BooLangService.Intellisense
{
    /// <summary>
    /// Converts between the Boo Antlr entity nodes and our tree-node structure.
    /// </summary>
    public class EntityToTreeNodeConverter
    {
        public IBooParseTreeNode ToTreeNode(IEntity entity)
        {
            if (entity is IType)
                return ToTreeNode((IType)entity);
            if (entity is IProperty)
                return ToTreeNode((IProperty)entity);
            if (entity is IMethod)
                return ToTreeNode((IMethod)entity);
            if (entity is INamespace)
                return ToTreeNode((INamespace)entity);

            return null;
        }

        private IBooParseTreeNode ToTreeNode(IType type)
        {
            if (type.IsInterface)
                return new InterfaceTreeNode(type, type.FullName) { Name = type.Name };
            if (type.IsClass || type.IsEnum)
                return new ClassTreeNode(type, type.FullName) { Name = type.Name };

            return null;
        }

        private IBooParseTreeNode ToTreeNode(INamespace @namespace)
        {
            var name = @namespace.ToString();

            // get the last part of the namespace name, because it's returned as
            // fully qualified
            if (name.Contains("."))
                name = name.Substring(name.LastIndexOf('.') + 1);

            return new ImportedNamespaceTreeNode { Name = name };
        }

        private IBooParseTreeNode ToTreeNode(IMethod method)
        {
            var member = new MethodTreeNode(method, method.ReturnType.ToString(), method.DeclaringType.FullName)
            {
                Name = method.Name
            };

            foreach (var parameter in method.GetParameters())
            {
                member.Parameters.Add(new MethodParameter
                {
                    Name = parameter.Name,
                    Type = parameter.Type.ToString()
                });
            }

            return member;
        }

        private IBooParseTreeNode ToTreeNode(IProperty property)
        {
            return new MethodTreeNode(property, property.GetGetMethod().ReturnType.ToString(), property.DeclaringType.FullName)
            {
                Name = property.Name
            };
        }
    }
}