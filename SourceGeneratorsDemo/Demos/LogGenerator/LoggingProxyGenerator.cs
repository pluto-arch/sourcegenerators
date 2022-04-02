using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LogGenerator
{
    [Generator]
    public class LoggingProxyGenerator: ISourceGenerator
    {
        /// <inheritdoc />
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        /// <inheritdoc />
        public void Execute(GeneratorExecutionContext context)
        {
            var compilation = context.Compilation;
            var mainMethod = compilation.GetEntryPoint(context.CancellationToken);
            var @namespace = mainMethod?.ContainingNamespace?.ToDisplayString()??"LoggingProxy";

            var syntaxReceiver = (SyntaxReceiver)context.SyntaxReceiver;
            var loggingTargets = syntaxReceiver?.TypeDeclarationsWithAttributes;

            if (loggingTargets == null)
            {
                return;
            }
            var logSrc = @$"namespace {@namespace}{{ public class LogAttribute:System.Attribute{{}} }}";
            context.AddSource("LogAttribute.cs", logSrc);

            var options = (CSharpParseOptions)compilation.SyntaxTrees.First().Options;
            var logSyntaxTree = CSharpSyntaxTree.ParseText(logSrc,options);
            compilation = compilation.AddSyntaxTrees(logSyntaxTree);
            var logAttribute = compilation.GetTypeByMetadataName($"{@namespace}.LogAttribute");
            var targetTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);


            foreach (var targetTypeSyntax in loggingTargets)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                var semanticModel = compilation.GetSemanticModel(targetTypeSyntax.SyntaxTree);
                var targetType = semanticModel.GetDeclaredSymbol(targetTypeSyntax);
                var hasLogAttribute = targetType?.GetAttributes().Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, logAttribute)) ?? false;
                if (!hasLogAttribute)
                    continue;
                if (targetTypeSyntax is not InterfaceDeclarationSyntax)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            "LG01",
                            "Log generator",
                            "[Log] 必须设置再接口上",
                            defaultSeverity: DiagnosticSeverity.Error,
                            severity: DiagnosticSeverity.Error,
                            isEnabledByDefault: true,
                            warningLevel: 0,
                            location: targetTypeSyntax.GetLocation()));
                    continue;
                }

                targetTypes.Add(targetType);
            }

            foreach (var targetType in targetTypes)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                var proxySource = GenerateProxy(targetType, @namespace);
                context.AddSource($"{targetType.Name}.Logging.cs", proxySource);
            }

        }

        private string GenerateProxy(ITypeSymbol targetType, string namespaceName)
        {
            var allInterfaceMethods = targetType.AllInterfaces
                .SelectMany(x => x.GetMembers())
                .Concat(targetType.GetMembers())
                .OfType<IMethodSymbol>()
                .ToList();

            var fullQualifiedName = GetFullQualifiedName(targetType);

            var sb = new StringBuilder();
            var proxyName = targetType.Name.Substring(1) + "LoggingProxy";
            sb.Append($@"using System;
                    using Microsoft.Extensions.Logging;

                    namespace {namespaceName}
                    {{
                          public static partial class LoggingExtensions
                          {{
                             public static {fullQualifiedName} WithLogging(this {fullQualifiedName} baseInterface,ILogger logger) => new {proxyName}(baseInterface,logger);
                          }}

                          public class {proxyName} : {fullQualifiedName}
                          {{
                                private readonly {fullQualifiedName} _target;
                                private readonly ILogger _logger;
                                public {proxyName}({fullQualifiedName} target,ILogger logger) {{_target = target;_logger=logger;}}");
                                foreach (var interfaceMethod in allInterfaceMethods)
                                {
                                    var containingType = interfaceMethod.ContainingType;
                                    var parametersList = string.Join(", ", interfaceMethod.Parameters.Select(x => $"{GetFullQualifiedName(x.Type)} {x.Name}"));
                                    var argumentList = string.Join(", ", interfaceMethod.Parameters.Select(x => x.Name));
                                    var isVoid = interfaceMethod.ReturnsVoid;
                                    var interfaceFullyQualifiedName = GetFullQualifiedName(containingType);

                                    sb.Append($@"
                                    {interfaceMethod.ReturnType} {interfaceFullyQualifiedName}.{interfaceMethod.Name}({parametersList})
                                    {{
                                        _logger.LogInformation({"\"method started Arguments: {@argumentLog}\""},{argumentList});
                                        try
                                        {{  ");
                                            if (!isVoid)
                                            {
                                                sb.Append("var result = ");
                                            }
                                            sb.AppendLine($"_target.{interfaceMethod.Name}({argumentList});");
                      sb.AppendLine($@"_logger.LogInformation({"\" {interfaceFullyQualifiedName}.{MethodName} finished result={@result}\""},""{interfaceFullyQualifiedName}"",""{interfaceMethod.Name}"",result);");
                                            if (!isVoid)
                                            {
                                                sb.AppendLine(" return result;");
                                            } 
                           sb.Append($@"}} catch (Exception e) {{ _logger.LogError(e,{$"$\"{interfaceMethod.Name} has an error: {{e.Message}}\""});throw; }}");
                       sb.Append($@"}}");
                                }
            sb.Append(@"}");
            sb.Append(@"}");
            return sb.ToString();
        }

        private static string GetFullQualifiedName(ISymbol symbol)
        {
            var containingNamespace = symbol.ContainingNamespace;
            if (!containingNamespace.IsGlobalNamespace)
                return containingNamespace.ToDisplayString() + "." + symbol.Name;

            return symbol.Name;
        }
    }


    public class SyntaxReceiver : ISyntaxReceiver
    {
        public HashSet<TypeDeclarationSyntax> TypeDeclarationsWithAttributes { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is TypeDeclarationSyntax declaration
                && declaration.AttributeLists.Any())
            {
                TypeDeclarationsWithAttributes.Add(declaration);
            }
        }
    }

}
