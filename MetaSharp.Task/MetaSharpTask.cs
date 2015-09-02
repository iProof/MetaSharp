﻿using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MetaSharp.Tasks {
    public class MetaSharpTask : ITask {
        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        [Required]
        public ITaskItem[] InputFiles { get; set; }
        [Required]
        public string IntermediateOutputPath { get; set; }
        [Output]
        public ITaskItem[] OutputFiles { get; set; }

        public bool Execute() {
            var files = InputFiles
                .Select(x => x.ItemSpec)
                .Where(x => x.EndsWith(".meta.cs"))
                .ToImmutableArray();
            var references = ImmutableArray.Create(typeof(object).Assembly.Location);
            var result = Generator.Generate(files, CreateEnvironment(), references);
            if(result.Errors.Any()) {
                foreach(var error in result.Errors) {
                    BuildEngine.LogErrorEvent(ToBuildError(error));
                }
                return false;
            } else {
                OutputFiles = result.Files
                    .Select(x => new TaskItem(x))
                    .ToArray();
                return true;
            }
            //List<SyntaxTree> trees = new List<SyntaxTree>();
            //List<string> files = new List<string>();
            //for(int i = 0; i < InputFiles.Length; i++) {
            //    if(InputFiles[i].ItemSpec.EndsWith(".meta.cs")) {
            //        trees.Add(SyntaxFactory.ParseSyntaxTree(File.ReadAllText(InputFiles[i].ItemSpec)));
            //        files.Add(InputFiles[i].ItemSpec);
            //    }
            //}

            //var compilation = CSharpCompilation.Create(
            //    "meta.dll",
            //    references: new[] {
            //        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            //    },
            //    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            //    syntaxTrees: trees
            //);

            ////var model = compilation.GetSemanticModel(compilation.SyntaxTrees.Single());
            ////model.SyntaxTree

            //var type = compilation.GlobalNamespace.GetNamespaceMembers().ElementAt(0).GetNamespaceMembers().ElementAt(0).GetTypeMembers().Single();
            //var tree = type.Locations.Single().SourceTree;
            //if(tree == compilation.SyntaxTrees.Single()) {
            //}
            //var node = compilation.SyntaxTrees.Single().GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            //var model = compilation.GetSemanticModel(compilation.SyntaxTrees.Single());
            //var symbol = model.GetDeclaredSymbol(node);
            //if(symbol == type) {
            //}

            //var errors = compilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error).ToArray();
            //if(errors.Any()) {
            //    foreach(var error in errors) {
            //        BuildEngine.LogErrorEvent(new BuildErrorEventArgs(
            //            subcategory: "MetaSharp",
            //            code: error.Id,
            //            file: files.Single(),
            //            lineNumber: error.Location.GetLineSpan().StartLinePosition.Line + 1,
            //            columnNumber: error.Location.GetLineSpan().StartLinePosition.Character + 1,
            //            endLineNumber: error.Location.GetLineSpan().EndLinePosition.Line + 1,
            //            endColumnNumber: error.Location.GetLineSpan().EndLinePosition.Character + 1,
            //            message: error.GetMessage(),
            //            helpKeyword: string.Empty,
            //            senderName: "MetaSharp"
            //            ));
            //    }
            //    return false;
            //}
            //Assembly compiledAssembly;
            //using(var stream = new MemoryStream()) {
            //    var compileResult = compilation.Emit(stream);
            //    compiledAssembly = Assembly.Load(stream.GetBuffer());
            //}
            //var result = (string)compiledAssembly.GetTypes().Single()
            //    .GetMethod("Do", BindingFlags.Static | BindingFlags.Public).Invoke(null, null);

            //OutputFiles = files.Select(x => new TaskItem(Path.Combine(IntermediateOutputPath, x.Replace(".meta.cs", ".meta.g.i.cs")))).ToArray();
            //File.WriteAllText(OutputFiles.Single().ItemSpec, result);
            //return true;
        }
        static BuildErrorEventArgs ToBuildError(GeneratorError error) {
            return new BuildErrorEventArgs(
                        subcategory: "MetaSharp",
                        code: error.Id,
                        file: error.File,
                        lineNumber: error.LineNumber + 1,
                        columnNumber: error.ColumnNumber + 1,
                        endLineNumber: error.EndLineNumber + 1,
                        endColumnNumber: error.EndColumnNumber + 1,
                        message: error.Message,
                        helpKeyword: string.Empty,
                        senderName: "MetaSharp"
                        );
        }
        Environment CreateEnvironment() {
            return new Environment(
                readText: fileName => File.ReadAllText(fileName),
                writeText: (fileName, text) => File.WriteAllText(fileName, text),
                loadAssembly: memoryStream => Assembly.Load(memoryStream.GetBuffer()),
                intermediateOutputPath: IntermediateOutputPath);
        }
    }
}