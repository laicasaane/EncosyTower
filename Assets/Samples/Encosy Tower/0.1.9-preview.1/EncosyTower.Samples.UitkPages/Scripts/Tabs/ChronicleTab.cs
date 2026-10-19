using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.PageFlows.UitkPages.Transitions;
using EncosyTower.Samples.UitkPages.App;
using EncosyTower.Samples.UitkPages.Attachments;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Ids;
using EncosyTower.Samples.UitkPages.PageUserData;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Tabs
{
    /// <summary>The Chronicle tab of a dimension screen.</summary>
    /// <remarks>
    /// A page of the Tabs flow, a single-page list attached to the screen.
    /// </remarks>
    [UxmlElement]
    public partial class ChronicleTab : UitkPageElement<AppFlowScopes>, IPageOnCreateAsync, IPageOnReturnToPool
    {
        private const string MOTIF_IMAGE = "image-motif";
        private const string BODY_LABEL = "label-body";

        private Image _motif;
        private Label _body;
        private readonly PaletteStyle _palette = new();

        private AttachmentSet _set;
        private Dimension? _motifId;

        /// <summary>Creates the page element.</summary>
        public ChronicleTab()
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
                _motif = this.Q<Image>(MOTIF_IMAGE);
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

            var surface = _palette.SurfaceOf(dimension.Index);

            style.backgroundColor = ToneMath.Analogous(surface, _palette).NearA;

            if (_motifId != dimension.Id)
            {
                _motifId = dimension.Id;
                _motif.image = Resources.Load<Texture2D>(dimension.MotifKey.Value);
            }

            _motif.tintColor = _palette.Color(ConfigColorType.Ink);
            _body.text = dimension.Lore;
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
