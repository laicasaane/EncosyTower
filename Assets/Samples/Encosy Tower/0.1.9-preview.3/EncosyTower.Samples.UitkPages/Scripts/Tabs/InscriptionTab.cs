using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.PageFlows.UitkPages.Transitions;
using EncosyTower.Samples.UitkPages.App;
using EncosyTower.Samples.UitkPages.Attachments;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Ids;
using EncosyTower.Samples.UitkPages.PageUserData;
using EncosyTower.Samples.UitkPages.Views;
using EncosyTower.Tasks;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Tabs
{
    /// <summary>The Inscription tab of a dimension screen.</summary>
    /// <remarks>
    /// A page of the Tabs flow, a single-page list attached to the screen.
    /// </remarks>
    [UxmlElement]
    public partial class InscriptionTab : UitkPageElement<AppFlowScopes>, IPageOnCreateAsync, IPageOnReturnToPool
    {
        private const string RUNE_VIEW = "rune";
        private const string BODY_LABEL = "label-body";

        private readonly PaletteStyle _palette = new();

        private RuneView _rune;
        private Label _body;
        private AttachmentSet _set;
        private Dimension? _foundId;
        private string _foundText;

        /// <summary>Creates the page element.</summary>
        public InscriptionTab()
        {
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        /// <inheritdoc/>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || PageUserDataAPI.TryGetAttachment(in context, out var set) == false)
            {
                return UnityTask.FromResult(false);
            }

            if (_body == null)
            {
                _rune = this.Q<RuneView>(RUNE_VIEW);
                _body = this.Q<Label>(BODY_LABEL);
            }

            _set = set;
            _set.Changed += OnSetChanged;

            Render();
            return UnityTask.FromResult(true);
        }

        /// <inheritdoc/>
        public void OnReturnToPool(PageContext context)
        {
            if (_set != null)
            {
                _set.Changed -= OnSetChanged;
                _set = null;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Fades the page root over the auxiliary transition time.
        /// </remarks>
        protected override IPageTransition CreateTransition(VisualElement slot)
            => new UitkValueAnimationTransition(slot) {
                ShowDurationMs = _set.State.Timings.AuxiliaryTransitionMs,
                HideDurationMs = _set.State.Timings.AuxiliaryTransitionMs,
            };

        private void Render()
        {
            if (_set.InUse == false || DimensionCatalog.TryGet(_set.Location, out var dimension) == false)
            {
                return;
            }

            var state = _set.State;
            var found = state.IsDiscovered(dimension.Index);

            style.backgroundColor = ToneMath.Analogous(_palette.SurfaceOf(dimension.Index), _palette).NearB;

            if (found)
            {
                if (_foundId != dimension.Id)
                {
                    _foundId = dimension.Id;
                    _foundText = state.Resources.Format(
                          ConfigTextType.TabInscriptionFound
                        , dimension.InscriptionObject
                    );
                }

                _rune.SetRune(state.RuneOf(dimension.Index), decoded: false, number: state.NumberOf(dimension.Index));
                _body.text = _foundText;
            }
            else
            {
                _rune.SetBlank();
                _body.text = state.Resources.Get(ConfigTextType.TabInscriptionEmpty);
            }

            _rune.SetInk(_palette.Color(ConfigColorType.Ink));
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            _palette.Read(evt.customStyle, ThemeAPI.IsDark(this));

            if (_set != null)
            {
                Render();
            }
        }

        private void OnSetChanged()
        {
            Render();
        }
    }
}
