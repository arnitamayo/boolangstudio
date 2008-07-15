using System.Collections.Generic;
using Boo.BooLangService.Document.Nodes;
using BooLangService;
using EnvDTE;
using VSLangProj;

namespace Boo.BooLangService.Document
{
    public class NamespaceFinder
    {
        /// <summary>
        /// Gets namespaces from references that match a search query.
        /// </summary>
        /// <param name="references">Project references to include in the search</param>
        /// <param name="searchQuery">Start of the namespace to resolve from. If blank gets all namespaces.</param>
        /// <returns>List of available namespaces</returns>
        public BooParseTreeNodeList QueryNamespacesFromReferences(References references, string searchQuery)
        {
            var namespaces = new BooParseTreeNodeList();

            foreach (Reference reference in references)
            {
                if (reference.SourceProject != null)
                    namespaces.AddRange(QueryNamespacesFromProjectReference(reference.SourceProject, searchQuery));
                else if (reference.Type == prjReferenceType.prjReferenceTypeAssembly)
                    namespaces.AddRange(QueryNamespacesFromAssembly(reference.Path, searchQuery));
            }

            return namespaces;
        }

        private IList<IBooParseTreeNode> QueryNamespacesFromProjectReference(Project project, string searchQuery)
        {
            var namespaces = new List<IBooParseTreeNode>();

            foreach (var ns in GetNamespacesFromCodeElements(project.CodeModel.CodeElements))
            {
                if (!ns.StartsWith(searchQuery)) continue;

                var scopedNamespace = ns.Remove(0, searchQuery.Length);

                if (scopedNamespace.Contains("."))
                    scopedNamespace = scopedNamespace.Substring(0, scopedNamespace.IndexOf("."));

                namespaces.Add(new ImportedNamespaceTreeNode { Name = scopedNamespace });
            }

            return namespaces;
        }

        private IEnumerable<string> GetNamespacesFromCodeElements(CodeElements codeElements)
        {
            var namespaces = new List<string>();

            foreach (CodeElement element in codeElements)
            {
                var codeNamespace = element as CodeNamespace;

                if (codeNamespace == null) continue;

                namespaces.AddRange(GetNamespacesFromCodeElements(codeNamespace.Members));

                bool isInternal = false;

                foreach (CodeElement member in codeNamespace.Members)
                {
                    if (member.InfoLocation == vsCMInfoLocation.vsCMInfoLocationProject)
                        isInternal = true;
                }

                if (isInternal)
                    namespaces.Add(codeNamespace.FullName);
            }

            return namespaces;
        }

        private IList<IBooParseTreeNode> QueryNamespacesFromAssembly(string path, string searchQuery)
        {
            var namespaces = new List<IBooParseTreeNode>();
            var assembly = AssemblyHelper.FindInCurrentAppDomainOrLoad(path);

            if (assembly == null) return namespaces; // just return empty list
            
            foreach (var type in assembly.GetExportedTypes())
            {
                if (!type.Namespace.StartsWith(searchQuery)) continue;

                string ns = type.Namespace;

                ns = ns.Remove(0, searchQuery.Length);

                if (ns.Contains("."))
                    ns = ns.Substring(0, ns.IndexOf('.'));

                namespaces.Add(new ImportedNamespaceTreeNode {Name = ns});
            }

            return namespaces;
        }
    }
}