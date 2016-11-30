using System;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CQSAnalyzer
{
    public class SilenceCQSAnalyzerAttribute : Attribute
    {
    }

    // TODO: add CQS to custom spelling dictionary

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CQSAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CQSAnalyzer";

        private const string Description = "This analyzer checks if your methods are violating Command-Query-Seperation.";

        private const string Category = "Style";
        internal const string CanTDetermineIfMethodShouldBeCommandOrQuery = "Can't determine if method should be command or query";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor("CQS1",
            "This analyzer checks if your methods are violating Command-Query-Seperation.",
            CanTDetermineIfMethodShouldBeCommandOrQuery,
            Category,
            DiagnosticSeverity.Error,
            true,
            Description);

        private static readonly DiagnosticDescriptor IllegalWriteAccessRule = new DiagnosticDescriptor("CQS2",
            "This write makes it wrong!",
            "This write makes it wrong!",
            Category,
            DiagnosticSeverity.Error,
            true,
            Description);

        private static readonly DiagnosticDescriptor IllegalWriteAccessRuleInfo = new DiagnosticDescriptor("CQS2",
            "This write makes it wrong!",
            "This write makes it wrong!",
            Category,
            DiagnosticSeverity.Info,
            true,
            Description);

        private static readonly DiagnosticDescriptor PotentiallyWrongCallRule = new DiagnosticDescriptor("CQS3",
            "Potential call to command from a query.",
            "Potential call to command from a query.",
            Category,
            DiagnosticSeverity.Warning,
            true,
            Description);

        private static readonly DiagnosticDescriptor PotentiallyWrongCallRuleInfo = new DiagnosticDescriptor("CQS3",
            "Potential call to command from a query.",
            "Potential call to command from a query.",
            Category,
            DiagnosticSeverity.Info,
            true,
            Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(IllegalWriteAccessRule, PotentiallyWrongCallRule, IllegalWriteAccessRuleInfo, PotentiallyWrongCallRuleInfo);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeLambda, SyntaxKind.ParenthesizedLambdaExpression, SyntaxKind.SimpleLambdaExpression);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            var reportAsInfo = false;
            var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node);
            if (symbol.HasIgnoreAttribute())
            {
                reportAsInfo = true;
            }


            if (methodDeclaration.IsVoidReturn())
            {
                // ist ein command
                return;
            }

            if (methodDeclaration.Body == null)
            {
                // empty body can't have errors
                return;
            }

            // ist eine query
            foreach (var statement in methodDeclaration.Body.Statements)
            {
                if (statement.Kind() == SyntaxKind.ExpressionStatement)
                {
                    var assignmentExpression = (statement as ExpressionStatementSyntax).Expression as AssignmentExpressionSyntax;
                    if (assignmentExpression != null)
                    {
                        var written = assignmentExpression.Left;
                        var s = context.SemanticModel.GetSymbolInfo(written).Symbol;
                        if (s.Kind != SymbolKind.Local && s.Kind != SymbolKind.Parameter)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(reportAsInfo ? IllegalWriteAccessRuleInfo : IllegalWriteAccessRule,
                                statement.GetLocation()));
                        }
                    }
                    else
                    {
                        // handle PostfixUnaryExpressionSyntax
                        var invocationExpression = (statement as ExpressionStatementSyntax).Expression as InvocationExpressionSyntax;
                        if (invocationExpression != null)
                        {
                            var hasCommentDisable = invocationExpression.GetLeadingTrivia()
                                                .Any(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) && t.ToString().Contains("disable"));
                            var methodCall = invocationExpression.Expression;
                            var s = context.SemanticModel.GetSymbolInfo(methodCall).Symbol;
                            var nodes = s.DeclaringSyntaxReferences;
                            var syntax = nodes.First().GetSyntax();
                            var mSyntax = syntax as MethodDeclarationSyntax;
                            var lSyntax = syntax as AnonymousFunctionExpressionSyntax;
                            if ((mSyntax != null && mSyntax.IsVoidReturn()) || (lSyntax != null && lSyntax.IsVoidReturn()))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(reportAsInfo || hasCommentDisable ? PotentiallyWrongCallRuleInfo : PotentiallyWrongCallRule,
                                    statement.GetLocation()));
                            }
                        }
                    }
                }
            }

            //if (methodDeclaration.GetDiagnostics().Any())
            //{
            //    context.ReportDiagnostic(Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation()));
            //}

            //if (methodDeclaration.IsVoidReturn())
            //{
            //    return;
            //}

            //if (methodDeclaration.HasNoParameters())
            //{
            //    return;
            //}

            //var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node);
            //if (symbol.HasPureAttribute())
            //{
            //    return;
            //}

            //context.ReportDiagnostic(Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation()));
        }

        private static void AnalyzeLambda(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (LambdaExpressionSyntax)context.Node;

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