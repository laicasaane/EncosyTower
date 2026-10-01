using EncosyTower.Data.Analyzers.Data;

namespace EncosyTower.SourceGen.Tests.Data;

[TestClass]
public sealed class DataFieldAttributeWithTargetsDiagnosticSuppressorTests
{
    [TestMethod]
    public Task DataSerializedFieldWithPropertyTarget_IsSuppressed()
        => SuppressorTestHelper.VerifyAsync<DataFieldAttributeWithTargetsDiagnosticSuppressor>(
            """
            using System;
            using EncosyTower.Data;
            using UnityEngine;

            [Data]
            public partial class Row
            {
                [SerializeField]
                [{|#0:property|}: Obsolete]
                public int value;
            }
            """,
            isSuppressed: true
        );

    [TestMethod]
    public Task SerializedFieldWithoutDataTypeMarker_IsNotSuppressed()
        => SuppressorTestHelper.VerifyAsync<DataFieldAttributeWithTargetsDiagnosticSuppressor>(
            """
            using System;
            using UnityEngine;

            public partial class Row
            {
                [SerializeField]
                [{|#0:property|}: Obsolete]
                public int value;
            }
            """,
            isSuppressed: false
        );

    [TestMethod]
    public Task DataFieldWithoutSerializeField_IsNotSuppressed()
        => SuppressorTestHelper.VerifyAsync<DataFieldAttributeWithTargetsDiagnosticSuppressor>(
            """
            using System;
            using EncosyTower.Data;

            [Data]
            public partial class Row
            {
                [{|#0:property|}: Obsolete]
                public int value;
            }
            """,
            isSuppressed: false
        );

    [TestMethod]
    public Task DataSerializedFieldWithWrongTarget_IsNotSuppressed()
        => SuppressorTestHelper.VerifyAsync<DataFieldAttributeWithTargetsDiagnosticSuppressor>(
            """
            using System;
            using EncosyTower.Data;
            using UnityEngine;

            [Data]
            public partial class Row
            {
                [SerializeField]
                [{|#0:event|}: Obsolete]
                public int value;
            }
            """,
            isSuppressed: false
        );
}
