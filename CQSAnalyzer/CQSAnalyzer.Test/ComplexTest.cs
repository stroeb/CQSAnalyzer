using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace CQSAnalyzer.Test
{
    [TestClass]
    public class ComplexTest : DiagnosticVerifier
    {
        [TestMethod]
        public void Method_QueryWithWrite_Violatio123n()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        public class Generic 
        {
           private int field;
public int Prop {get;set;}
            public void QueryWithWrite(int p1, ref int p_ref, out int p_out)
            {
                var l1 = 0;
                int l2 = 0;

l1 = 2;
l2 = 2;
p1 = 2;
p_ref = 2;
p_out = 2;
field = 2;
Prop = 2;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = "CQSAnalyzer",
                Message = CQSAnalyzerAnalyzer.CanTDetermineIfMethodShouldBeCommandOrQuery,
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
        public void Method_QueryWithWrite_Violation()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        public class Generic 
        {
           private int a;
            public int QueryWithWrite()
            {
var z = 0;
                z++;
a = 6;
                return z;
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = "CQSAnalyzer",
                Message = "Can't determine if method should be command or query",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 22)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CQSAnalyzerAnalyzer();
        }
    }
}