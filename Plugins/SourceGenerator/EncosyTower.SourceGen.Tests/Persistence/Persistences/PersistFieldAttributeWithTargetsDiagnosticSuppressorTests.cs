using EncosyTower.Persistence.Analyzers;

namespace EncosyTower.SourceGen.Tests.Persistence.Persistences;

[TestClass]
public sealed class PersistFieldAttributeWithTargetsDiagnosticSuppressorTests
{
    [DataTestMethod]
    [DataRow("id")]
    [DataRow("_id")]
    [DataRow("m_id")]
    [DataRow("version")]
    [DataRow("_version")]
    [DataRow("m_version")]
    public Task PersistFieldWithSupportedName_IsSuppressed(string fieldName)
        => SuppressorTestHelper.VerifyAsync<PersistFieldAttributeWithTargetsDiagnosticSuppressor>(
            $$"""
            using System;
            using EncosyTower.Persistences;

            [Persist]
            public partial class SaveData
            {
                [{|#0:property|}: Obsolete]
                public string {{fieldName}};
            }
            """,
            isSuppressed: true
        );

    [TestMethod]
    public Task PersistFieldWithUnsupportedName_IsNotSuppressed()
        => SuppressorTestHelper.VerifyAsync<PersistFieldAttributeWithTargetsDiagnosticSuppressor>(
            """
            using System;
            using EncosyTower.Persistences;

            [Persist]
            public partial class SaveData
            {
                [{|#0:property|}: Obsolete]
                public string name;
            }
            """,
            isSuppressed: false
        );

    [TestMethod]
    public Task SupportedFieldWithoutPersistTypeMarker_IsNotSuppressed()
        => SuppressorTestHelper.VerifyAsync<PersistFieldAttributeWithTargetsDiagnosticSuppressor>(
            """
            using System;

            public partial class SaveData
            {
                [{|#0:property|}: Obsolete]
                public string id;
            }
            """,
            isSuppressed: false
        );

    [TestMethod]
    public Task PersistFieldWithWrongTarget_IsNotSuppressed()
        => SuppressorTestHelper.VerifyAsync<PersistFieldAttributeWithTargetsDiagnosticSuppressor>(
            """
            using System;
            using EncosyTower.Persistences;

            [Persist]
            public partial class SaveData
            {
                [{|#0:event|}: Obsolete]
                public string id;
            }
            """,
            isSuppressed: false
        );

    [DataTestMethod]
    [DataRow("PersistAttribute")]
    [DataRow("EncosyTower.Persistences.PersistAttribute")]
    public Task PersistFieldWithSuffixedTypeMarker_IsSuppressed(string marker)
        => SuppressorTestHelper.VerifyAsync<PersistFieldAttributeWithTargetsDiagnosticSuppressor>(
              $$"""
              using System;
              using EncosyTower.Persistences;

              [{{marker}}]
              public partial class SaveData
              {
                  [{|#0:property|}: Obsolete]
                  public string id;
              }
              """
            , isSuppressed: true
        );

    [TestMethod]
    public Task PersistFieldInUnmarkedPartialPart_IsSuppressed()
        => SuppressorTestHelper.VerifyAsync<PersistFieldAttributeWithTargetsDiagnosticSuppressor>(
              """
              using System;
              using EncosyTower.Persistences;

              [Persist]
              public partial class SaveData { }

              public partial class SaveData
              {
                  [{|#0:property|}: Obsolete]
                  public string id;
              }
              """
            , isSuppressed: true
        );

    [TestMethod]
    public Task SupportedFieldWithUnrelatedPersistAttribute_IsNotSuppressed()
        => SuppressorTestHelper.VerifyAsync<PersistFieldAttributeWithTargetsDiagnosticSuppressor>(
              """
              using System;

              namespace Unrelated
              {
                  [AttributeUsage(AttributeTargets.Class)]
                  public sealed class PersistAttribute : Attribute { }
              }

              namespace TestProject
              {
                  using Unrelated;

                  [PersistAttribute]
                  public partial class SaveData
                  {
                      [{|#0:property|}: Obsolete]
                      public string id;
                  }
              }
              """
            , isSuppressed: false
        );
}
