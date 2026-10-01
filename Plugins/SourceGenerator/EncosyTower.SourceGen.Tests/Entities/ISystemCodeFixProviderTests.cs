using EncosyTower.Entities.CodeRefactors;

namespace EncosyTower.SourceGen.Tests.Entities;

[TestClass]
public sealed class ISystemCodeFixProviderTests
{
    [TestMethod]
    public Task CompleteContract_AppliesExactFixAndFixAll()
        => CodeFixTestHelper.VerifyAsync<ISystemDiagnosticAnalyzer, ISystemCodeFixProvider>(
              """
              using EncosyTower.Entities;

              [ISystem]
              public partial struct {|SG_ISYSTEM_0001:GameSystem|} { }
              """.Replace("\n", "\r\n")
            , """
              using EncosyTower.Entities;

              [ISystem]
              public partial struct GameSystem : global::Unity.Entities.ISystem {

                  public void OnCreate(ref global::Unity.Entities.SystemState state)
                  {
                  }


                  public void OnUpdate(ref global::Unity.Entities.SystemState state)
                  {
                  }


                  public void OnDestroy(ref global::Unity.Entities.SystemState state)
                  {
                  }
              }
              """.Replace("\n", "\r\n")
            , "Implement missing ISystem members"
        );

    [TestMethod]
    public async Task EveryAction_HasItsExactStableEquivalenceKey()
    {
        var provider = new ISystemCodeFixProvider();
        CollectionAssert.AreEqual(new[] { "SG_ISYSTEM_0001" }, provider.FixableDiagnosticIds.ToArray());
        Assert.IsNotNull(provider.GetFixAllProvider());

        var actions = await CodeFixTestHelper
            .GetActionsAsync<ISystemDiagnosticAnalyzer, ISystemCodeFixProvider>(
                """
                using EncosyTower.Entities;

                [ISystem]
                public partial struct GameSystem { }
                """
            );
        var expected = new[] {
            "Implement missing ISystem members",
            "Implement ISystem.OnCreate",
            "Implement ISystem.OnUpdate",
            "Implement ISystem.OnDestroy",
        };

        CollectionAssert.AreEqual(expected, actions.Select(static action => action.Title).ToArray());
        CollectionAssert.AreEqual(
              expected
            , actions.Select(static action => action.EquivalenceKey).ToArray()
        );
    }
}
