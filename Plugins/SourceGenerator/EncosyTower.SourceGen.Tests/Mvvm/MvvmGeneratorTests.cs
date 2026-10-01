using EncosyTower.Mvvm.Generators.Binders;
using EncosyTower.Mvvm.Generators.InternalStringAdapters;
using EncosyTower.Mvvm.Generators.InternalVariants;
using EncosyTower.Mvvm.Generators.MonoBinders;
using EncosyTower.Mvvm.Generators.ObservableProperties;
using EncosyTower.Mvvm.Generators.RelayCommands;

namespace EncosyTower.SourceGen.Tests.Mvvm;

[TestClass]
public class ObservablePropertyGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<ObservablePropertyGenerator>();

    [TestMethod]
    public Task SkipAttribute_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<ObservablePropertyGenerator>(
            """
            using EncosyTower.Mvvm.ComponentModel;

            [assembly: EncosyTower.Mvvm.SkipSourceGeneratorsForAssembly]

            namespace TestProject;

            [ObservableObject]
            public partial class Model
            {
                [ObservableProperty]
                private int _value;

                public int Value { get => _value; set => _value = value; }
            }
            """
        );

    [TestMethod]
    public Task ObservableMembers_GenerateObservableObject()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<ObservablePropertyGenerator>(
              """
              using EncosyTower.Mvvm.ComponentModel;

              namespace TestProject;

              [ObservableObject]
              public partial class Model
              {
                  [ObservableProperty]
                  [NotifyPropertyChangedFor(nameof(Progress))]
                  private int _value;

                  [ObservableProperty]
                  public string Name { get => Get_Name(); set => Set_Name(value); }

                  public float Progress => _value / 100f;
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<ObservablePropertyGenerator>(
                    "Model.ObservableObject.438b7d6b2b97beeb.g.cs"
                ),
            }
        );

    [TestMethod]
    public Task NoObservableMembers_GeneratesEmptyObservableObject()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<ObservablePropertyGenerator>(
              """
              using EncosyTower.Mvvm.ComponentModel;

              namespace TestProject;

              [ObservableObject]
              public partial class Model { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<ObservablePropertyGenerator>(
                    "Model.ObservableObject.438b7d6b2b97beeb.g.cs"
                ),
            }
        );
}

[TestClass]
public class RelayCommandGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<RelayCommandGenerator>();

    [TestMethod]
    public Task RelayCommandMethods_GenerateCommands()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<RelayCommandGenerator>(
              """
              using EncosyTower.Mvvm.ComponentModel;
              using EncosyTower.Mvvm.Input;

              namespace TestProject;

              [ObservableObject]
              public partial class Model
              {
                  [RelayCommand]
                  private void OnSave() { }

                  [RelayCommand(CanExecute = nameof(CanProcess))]
                  private void OnProcess(int value) { }

                  private bool CanProcess(int value) => value > 0;
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<RelayCommandGenerator>(
                    "Model.RelayCommand.3d8df8b40b7b3be5.g.cs"
                ),
            }
        );
}

[TestClass]
public class BinderGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<BinderGenerator>();

    [TestMethod]
    public Task BindingMembers_GenerateBinder()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<BinderGenerator>(
              """
              using EncosyTower.Mvvm.ViewBinding;

              namespace TestProject;

              [Binder]
              public partial class SampleBinder
              {
                  [BindingProperty]
                  private void SetValue(int value) { }

                  [BindingCommand]
                  partial void OnClick();
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<BinderGenerator>(
                    "SampleBinder.Binder.fa06092ce235dd05.g.cs"
                ),
            }
        );
}

[TestClass]
public class MonoBinderGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<MonoBinderGenerator>();

    [TestMethod]
    public Task GameObjectComponent_GeneratesMonoBinder()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<MonoBinderGenerator>(
              """
              using EncosyTower.Mvvm.ViewBinding.Components;

              namespace TestProject;

              [MonoBinder(typeof(UnityEngine.GameObject))]
              public partial class GameObjectBinder { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<MonoBinderGenerator>(
                    "GameObjectBinder.MonoBinder.c51c76287ebe5a95.g.cs"
                ),
            }
        );
}

[TestClass]
public class InternalVariantGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<InternalVariantGenerator>();

    [TestMethod]
    public Task ObservablePropertyType_GeneratesInternalVariant()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<InternalVariantGenerator>(
              """
              using EncosyTower.Mvvm.ComponentModel;

              namespace TestProject;

              public struct Score
              {
                  public int value;
              }

              [ObservableObject]
              public partial class Model
              {
                  [ObservableProperty]
                  private Score _score;

                  public Score Current { get => _score; set => _score = value; }
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<InternalVariantGenerator>(
                    "Score.InternalVariant.8fef2e37c5067e33.g.cs"
                ),
                ExpectedGeneratedSource.Create<InternalVariantGenerator>(
                    "Input.InternalVariantRegistry.8c32bd889fc6bbf1.g.cs"
                ),
            }
        );
}

[TestClass]
public class InternalStringAdapterGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<InternalStringAdapterGenerator>();

    [TestMethod]
    public Task ObservablePropertyType_GeneratesStringAdapter()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<InternalStringAdapterGenerator>(
              """
              using EncosyTower.Mvvm.ComponentModel;

              namespace TestProject;

              public struct Score
              {
                  public int value;
              }

              [ObservableObject]
              public partial class Model
              {
                  [ObservableProperty]
                  private Score _score;

                  public Score Current { get => _score; set => _score = value; }
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<InternalStringAdapterGenerator>(
                    "Input.InternalStringAdapters.fa51c64abefa4b53.g.cs"
                ),
            }
        );

    [TestMethod]
    public Task GenericPropertyType_GeneratesStringAdapterWithLegacyName()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<InternalStringAdapterGenerator>(
              """
              using System.Collections.Generic;
              using EncosyTower.Mvvm.ComponentModel;

              namespace TestProject;

              [ObservableObject]
              public partial class Model
              {
                  [ObservableProperty]
                  private List<int> _items;

                  public List<int> Items { get => _items; set => _items = value; }
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<InternalStringAdapterGenerator>(
                    "Input.InternalStringAdapters.fa51c64abefa4b53.g.cs"
                ),
            }
        );
}
