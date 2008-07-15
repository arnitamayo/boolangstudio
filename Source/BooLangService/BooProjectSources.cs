using System;
using System.Collections.Generic;
using System.IO;
using Boo.BooLangService.Document;
using BooLangService;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell.Interop;

namespace Boo.BooLangProject
{
    /// <summary>
    /// Represents a Boo project, in terms of source files and parsing.
    /// Acts as a facade over the boo compiler, to allow easier compilation of
    /// all the Boo files in a project.
    /// </summary>
    public class BooProjectSources
    {
        private static readonly IList<BooProjectSources> loadedProjects = new List<BooProjectSources>();
        private readonly Dictionary<string, int> files = new Dictionary<string, int>(); // filename, lastUpdate
        private readonly HierarchyListener hierarchyListener;
        private CompiledProject compiledProject;

        public BooProjectSources(IVsHierarchy hierarchy)
        {
            hierarchyListener = new HierarchyListener(hierarchy);
            hierarchyListener.ItemAdded += hierarchyListener_ItemAdded;
            hierarchyListener.StartListening();
        }

        void hierarchyListener_ItemAdded(object sender, HierarchyEventArgs e)
        {
            files.Add(e.FileName, 1);
        }

        /// <summary>
        /// Static collection of projects that've been loaded into the currently opened
        /// solution.
        /// </summary>
        public static IList<BooProjectSources> LoadedProjects
        {
            get { return loadedProjects; }
        }

        /// <summary>
        /// Finds a project in the loaded projects collection that contains the
        /// specified file.
        /// </summary>
        /// <param name="file">Path of a file to find in a project.</param>
        /// <returns>Project containing specified file.</returns>
        public static BooProjectSources Find(string file)
        {
            foreach (var project in LoadedProjects)
            {
                if (project.HasFile(file))
                    return project;
            }

            return null;
        }

        /// <summary>
        /// Gets the compiled output for the current project, compiling it if needed.
        /// </summary>
        /// <returns>Compiled project.</returns>
        public CompiledProject GetCompiledProject()
        {
            if (RequiresRecompilation)
                Compile();

            return compiledProject;
        }

        /// <summary>
        /// Updates the compiled project when a parse request is raised.
        /// </summary>
        /// <param name="request">Parse request that raised the update.</param>
        public void Update(ParseRequest request)
        {
            string filePath = request.FileName;
            int requestTimestamp = request.Timestamp;
            int lastUpdated = files[filePath];

            if (requestTimestamp != lastUpdated) // TODO: should check if any files have changed here
            {
                ResetCompiledProject();
                files[filePath] = requestTimestamp;
            }
        }

        /// <summary>
        /// Checks whether a project contains the specified file.
        /// </summary>
        /// <param name="fileName">File name to find.</param>
        /// <returns>Whether a project contains the file.</returns>
        private bool HasFile(string fileName)
        {
            return files.ContainsKey(fileName);
        }

        /// <summary>
        /// Resets the compiled project, so it'll be recompiled on the next call.
        /// </summary>
        private void ResetCompiledProject()
        {
            compiledProject = null;
        }

        /// <summary>
        /// Iterates the files and compiles them en-masse.
        /// </summary>
        private void Compile()
        {
            var compiler = new BooDocumentCompiler();

            foreach (var fileName in files.Keys)
            {
                var source = GetSource(fileName);

                compiler.AddSource(fileName, source);
            }

            compiledProject = compiler.Compile();
        }

        /// <summary>
        /// Gets the contents of a file.
        /// </summary>
        /// <param name="fileName">File to get the source of</param>
        /// <returns></returns>
        private string GetSource(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        /// <summary>
        /// Whether the project requires recompilation.
        /// </summary>
        private bool RequiresRecompilation
        {
            get { return compiledProject == null; }
        }
    }
}