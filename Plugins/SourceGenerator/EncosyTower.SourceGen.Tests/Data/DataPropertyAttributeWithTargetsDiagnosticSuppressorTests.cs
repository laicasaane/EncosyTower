using EncosyTower.Data.Analyzers.Data;

namespace EncosyTower.SourceGen.Tests.Data;

[TestClass]
public sealed class DataPropertyAttributeWithTargetsDiagnosticSuppressorTests
{
    [TestMethod]
    public Task DataPropertyWithFieldTarget_IsSuppressed()
        => SuppressorTestHelper.VerifyAsync<DataPropertyAttributeWithTargetsDiagnosticSuppressor>(
            """
            using System;
            using EncosyTower.Data;

            public partial class Row
            {
                [DataProperty]
                [{|#0:field|}: Obsolete]
                public int Value => 0;
            }
            """,
            isSuppressed: true
        );

    [TestMethod]
    public Task PropertyWithoutDataPropertyMarker_IsNotSuppressed()
        => SuppressorTestHelper.VerifyAsync<DataPropertyAttributeWithTargetsDiagnosticSuppressor>(
            """
            using System;

            public partial class Row
            {
                [{|#0:field|}: Obsolete]
                public int Value => 0;
            }
            """,
            isSuppressed: false
        );

    [TestMethod]
    public Task DataPropertyWithWrongTarget_IsNotSuppressed()
        => SuppressorTestHelper.VerifyAsync<DataPropertyAttributeWithTargetsDiagnosticSuppressor>(
            """
            using System;
            using EncosyTower.Data;

            public partial class Row
            {
                [DataProperty]
                [{|#0:method|}: Obsolete]
                public int Value => 0;
            }
            """,
            isSuppressed: false
        );
}
