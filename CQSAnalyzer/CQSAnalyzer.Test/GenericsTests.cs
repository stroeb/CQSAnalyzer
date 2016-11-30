using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace CQSAnalyzer.Test
{
    [TestClass]
    public class GenericsTests : DiagnosticVerifier
    {
        [TestMethod]
        public void GenericMethod_IsCommandAndQuery_Violation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        public class Generic 
        {
            public T GenericCommandQuery<T>(T para)
            {
                return default(T);
            }
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
                        new DiagnosticResultLocation("Test0.cs", 8, 22)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void GenericMethod_IsCommand_NoViolation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        public class Generic 
        {
            private void GenericCommand<T>(T para)
            {
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void GenericMethod_IsQuery_NoViolation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        public class Generic 
        {
            public T GenericQuery<T>()
            {
                return default(T);
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