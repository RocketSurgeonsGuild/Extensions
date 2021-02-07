using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;

namespace Rocket.Surgery.Experimental.Analyzers.Tests.Helpers
{
    public static class GenerationHelpers
    {
        internal const string CrLf = "\r\n";
        internal const string Lf = "\n";
        internal const string DefaultFilePathPrefix = "Test";
        internal const string CSharpDefaultFileExt = "cs";
        internal const string TestProjectName = "TestProject";

        public static Project CreateProject(
            string projectName,
            IEnumerable<MetadataReference> metadataReferences,
            params SourceText[] sources)
        {
            var projectId = ProjectId.CreateNewId(projectName);
            var solution = new AdhocWorkspace()
               .CurrentSolution
               .AddProject(projectId, projectName, projectName, LanguageNames.CSharp)
               .WithProjectCompilationOptions(
                    projectId,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                )
               .WithProjectParseOptions(
                    projectId,
                    new CSharpParseOptions(preprocessorSymbols: new[] { "SOMETHING_ACTIVE" })
                )
               .AddMetadataReferences(projectId, metadataReferences);

            var count = 0;
            foreach (var source in sources)
            {
                var newFileName = DefaultFilePathPrefix + count + "." + CSharpDefaultFileExt;
                var documentId = DocumentId.CreateNewId(projectId, newFileName);
                solution = solution.AddDocument(documentId, newFileName, source);
                count++;
            }

            var project = solution.GetProject(projectId);
            if (project is null)
            {
                throw new InvalidOperationException($"The ad hoc workspace does not contain a project with the id {projectId.Id}");
            }

            return project;
        }

        public static Project CreateProject(IEnumerable<MetadataReference> metadataReferences, params SourceText[] sources)
            => CreateProject(TestProjectName, metadataReferences, sources);
    }
}