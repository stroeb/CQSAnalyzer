using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace CQSAnalyzer.Test
{
    [TestClass]
    public class InstanceMethodsTests : DiagnosticVerifier
    {
        [TestMethod]
        public void InstanceMethod_IsCommandAndQuery_Violation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        public class Generic 
        {
            public int CommandQuery(int z)
            {
                return 0;
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
                        new DiagnosticResultLocation("Test0.cs", 8, 24)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void InstanceMethod_IsCommandAndQueryExpression_Violation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        public class Generic 
        {
            public int CommandQueryExpression(int z) => 0;
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
                        new DiagnosticResultLocation("Test0.cs", 8, 24)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void InstanceMethod_IsCommand_NoViolation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        public class Generic 
        {
            public void Command(string u)
            {
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void InstanceMethod_IsQuery_NoViolation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        public class Generic 
        {
            private int Query()
            {
                return 0;
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void InstanceMethod_IsEmpty_NoViolation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        public class Generic 
        {
            private void Void()
            {
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void InstanceMethod_IsCommandAndQueryAndHasJetBrainsPureAttribute_NoViolation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        public class Generic 
        {
            [JetBrains.Annotations.Pure]
            private bool PurePredicateJetBrains(string a)
            {
                return false;
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void InstanceMethod_IsCommandAndQueryAndHasContractsPureAttribute_NoViolation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        public class Generic 
        {
            [System.Diagnostics.Contracts.Pure]
            private bool PurePredicateContracts(string a)
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