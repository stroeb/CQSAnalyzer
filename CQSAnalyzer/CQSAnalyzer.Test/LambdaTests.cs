using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace CQSAnalyzer.Test
{
    [TestClass]
    public class LambdaTests : DiagnosticVerifier
    {
        [TestMethod]
        public void ParenthesizedLambda_IsCommandAndQuery_Violation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        static void Main(string[] args)
        {
            Func<int, int> commandAndQuery = (int r) => { return 0; };
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "CQSAnalyzer",
                Message = CQSAnalyzerAnalyzer.MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 46)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void SimpleLambda_IsCommandAndQuery_Violation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        static void Main(string[] args)
        {
            Func<int, int> commandAndQuerySimple = r => { return 0; };
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "CQSAnalyzer",
                Message = CQSAnalyzerAnalyzer.MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 52)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void ParenthesizedLambda_IsCommand_NoViolation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        static void Main(string[] args)
        {
            Action<int> command = (int r) => { };
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void SimpleLambda_IsCommand_NoViolation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        static void Main(string[] args)
        {
            Action<int> commandSimple = r => { };
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void ParenthesizedLambda_IsQuery_NoViolation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        static void Main(string[] args)
        {
            Func<int> query = () => { return 0; };
        }
    }";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void ParenthesizedLambda_IsEmptyQuery_NoViolation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        static void Main(string[] args)
        {
            Action v = () => { };
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