using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CQSAnalyzer
{
    internal static class Extensions
    {
        private const string SystemDiagnosticsContractsPureAttribute = "System.Diagnostics.Contracts.PureAttribute";
        private const string JetbrainsAnnotationsPureAttribute = "JetBrains.Annotations.PureAttribute";

        public static bool HasPureAttribute(this ISymbol declaredSymbol)
        {
            var attributes = declaredSymbol.GetAttributes();
            return attributes.Any(a => a.ToString() == SystemDiagnosticsContractsPureAttribute || a.ToString() == JetbrainsAnnotationsPureAttribute);
        }

        public static bool IsVoidReturn(this MethodDeclarationSyntax methodDeclaration)
        {
            var nonGenericReturn = methodDeclaration.ReturnType as PredefinedTypeSyntax;
            return nonGenericReturn != null && nonGenericReturn.Keyword.Kind() == SyntaxKind.VoidKeyword;
        }

        public static bool IsVoidReturn(this AnonymousFunctionExpressionSyntax methodDeclaration)
        {
            return !((BlockSyntax)methodDeclaration.Body).Statements.Any(SyntaxKind.ReturnStatement);
        }

        public static bool HasNoParameters(this MethodDeclarationSyntax methodDeclaration)
        {
            return !methodDeclaration.ParameterList.Parameters.Any(p => p.Modifiers.All(pm => pm.Kind() != SyntaxKind.ThisKeyword));
        }

        public static bool HasNoParameters(this SimpleLambdaExpressionSyntax methodDeclaration)
        {
            return methodDeclaration.Parameter == null;
        }

        public static bool HasNoParameters(this ParenthesizedLambdaExpressionSyntax methodDeclaration)
        {
            return !methodDeclaration.ParameterList.Parameters.Any();
        }
    }
}