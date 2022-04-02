using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace LogGenerator
{
    [Generator]
    public class ControllerGenerator:ISourceGenerator
    {
        /// <inheritdoc />
        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
        }

        /// <inheritdoc />
        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxTree = context.Compilation.SyntaxTrees;
            var handles = syntaxTree.Where(x => x.GetText(context.CancellationToken).ToString().Contains("AppService"));
            var compilation = context.Compilation;
            var mainMethod = compilation.GetEntryPoint(context.CancellationToken);
            var @namespace = mainMethod?.ContainingNamespace?.ToDisplayString()??"LoggingProxy";
            foreach (var handle in handles)
            {
                var ss = handle.GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>();
                var @usings = handle.GetRoot(context.CancellationToken).DescendantNodes().OfType<UsingDirectiveSyntax>();
                var @usingsText = string.Join("\r\n", @usings);
                var sourceBuilder = new StringBuilder();

                var classDeclar = handle.GetRoot(context.CancellationToken).DescendantNodes()
                    .OfType<ClassDeclarationSyntax>();

                foreach (var @class in classDeclar)
                {

                    var attrs = @class.AttributeLists;
                    var @attrsText = string.Join("\r\n", attrs);

                    var className = @class.Identifier.ToString();
                    if (!className.EndsWith("AppService"))
                    {
                        continue;
                    }
                    var generateClassName = $"{className}Controller";
                    var splitClass = @class.ToString().Split(new[] {'{'}, 2);
                    sourceBuilder.Append(@usingsText);
                    sourceBuilder.AppendLine();
                    sourceBuilder.Append($@"namespace {@namespace} 
{{
    [ApiController]
    {@attrsText}
    public class {generateClassName}:ControllerBase
    {{
");
                    sourceBuilder.AppendLine(splitClass[1].Replace(className,generateClassName));
                    sourceBuilder.AppendLine("}");
                    context.AddSource($"{generateClassName}.g.cs",SourceText.From(sourceBuilder.ToString(),Encoding.UTF8));
                }
            }

        }


        
        private static string GetFullQualifiedName(ISymbol symbol)
        {
            var containingNamespace = symbol.ContainingNamespace;
            if (!containingNamespace.IsGlobalNamespace)
                return containingNamespace.ToDisplayString() + "." + symbol.Name;

            return symbol.Name;
        }
    }
}

