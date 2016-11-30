using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace CQSAnalyzer.Test
{
    [TestClass]
    public class ExtensionMethodTests : DiagnosticVerifier
    {
        [TestMethod]
        public void ExtensionMethod_IsQuery_NoViolation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        public static class Extensions 
        {
            public static bool ExtensionQueryMethod(this string param)
            { 
                return false;
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void ExtensionMethod_IsCommand_NoViolation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        public static class Extensions 
        {
           public static void ExtensionCommandMethod(this string param, int u)
           {
           }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void ExtensionMethod_HasAdditionalParam_Violation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        public static class Extensions 
        {
            public static bool ExtensionCommandAndQueryMethod(this string param, int u)
            {
                return false;
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = CQSAnalyzerAnalyzer.DiagnosticId,
                Message = "Can't determine if method should be command or query",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 32)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void ExtensionMethod_HasAdditionalParamAndHasJetBrainsPureAttribute_NoViolation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        public static class Extensions 
        {
            [JetBrains.Annotations.Pure]
            public static bool ExtensionCommandAndQueryMethod(this string param, int u)
            {
                return false;
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void ExtensionMethod_HasAdditionalParamAndHasContractsPureAttribute_NoViolation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        public static class Extensions 
        {
            [System.Diagnostics.Contracts.Pure]
            public static bool ExtensionCommandAndQueryMethod(this string param, int u)
            {
                return false;
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CQSAnalyzerAnalyzer();
        }
    }
}