using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CQSAnalyzer
{
    // TODO: add CQS to custom spelling dictionary

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CQSAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CQSAnalyzer";

        private const string Title = "This analyzer checks if your methods are violating Command-Query-Seperation.";

        private const string MessageFormat = "Method is Command AND Query!";

        private const string Description = "This analyzer checks if your methods are violating Command-Query-Seperation.";

        private const string Category = "Style";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            true,
            Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeSimpleLambda, SyntaxKind.SimpleLambdaExpression);
            context.RegisterSyntaxNodeAction(AnalyzeLambda, SyntaxKind.ParenthesizedLambdaExpression);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (methodDeclaration.IsVoidReturn())
            {
                return;
            }

            if (methodDeclaration.HasNoParameters())
            {
                return;
            }

            var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node);
            if (symbol.HasPureAttribute())
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation()));
        }

        private static void AnalyzeSimpleLambda(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (SimpleLambdaExpressionSyntax)context.Node;

            if (methodDeclaration.IsVoidReturn())
            {
                return;
            }

            if (methodDeclaration.HasNoParameters())
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, methodDeclaration.GetLocation()));
        }

        private static void AnalyzeLambda(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (ParenthesizedLambdaExpressionSyntax)context.Node;

            if (methodDeclaration.IsVoidReturn())
            {
                return;
            }

            if (methodDeclaration.HasNoParameters())
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, methodDeclaration.GetLocation()));
        }
    }
}