using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.Samples.UguiPages.App;
using EncosyTower.Samples.UguiPages.Attachments;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.PageUserData;
using EncosyTower.Samples.UguiPages.State;
using EncosyTower.Samples.UguiPages.Views;
using EncosyTower.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.UguiPages.Tabs
{
    /// <summary>The Chronicle tab of a dimension screen.</summary>
    /// <remarks>A page of the Tabs flow, a single-page list attached to the screen.</remarks>
    public class ChronicleTab : UguiPageBehaviour<AppFlowScopes>, IPageOnCreateAsync, IPageOnReturnToPool
    {
        /// <summary>The background image of the tab.</summary>
        [Tooltip("The background image of the tab.")]
        [SerializeField] private Image _background;

        /// <summary>The motif image of the tab.</summary>
        [Tooltip("The motif image of the tab.")]
        [SerializeField] private Image _motif;

        /// <summary>The title of the tab.</summary>
        [Tooltip("The title of the tab.")]
        [SerializeField] private TMP_Text _title;

        /// <summary>The body text of the tab.</summary>
        [Tooltip("The body text of the tab.")]
        [SerializeField] private TMP_Text _body;

        private AttachmentSet _set;
        private AppState _state;
        private Dimension? _motifId;

        /// <inheritdoc/>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || PageUserDataAPI.TryGetSet(in context, out var set) == false)
            {
                return UnityTask.FromResult(false);
            }

            _set = set;
            _set.Rebound += OnStateChanged;
            _state = set.State;
            _state.Changed += OnStateChanged;

            Render();
            return UnityTask.FromResult(true);
        }

        /// <inheritdoc/>
        public void OnReturnToPool(PageContext context)
        {
            Unbind();
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void Unbind()
        {
            if (_set != null)
            {
                _set.Rebound -= OnStateChanged;
                _set = null;
            }

            if (_state != null)
            {
                _state.Changed -= OnStateChanged;
                _state = null;
            }
        }

        private void Render()
        {
            if (_set == null
                || _set.InUse == false
                || DimensionCatalog.TryGet(_set.Location, out var dimension) == false
                || dimension.Index >= DimensionCatalog.OUTER_COUNT
            )
            {
                return;
            }

            var config = _state.Config;
            var ink = config.Color(ConfigColorType.Ink);

            _background.color = ToneMath.Analogous(config, config.Color(dimension.Surface)).NearA;

            if (_motifId != dimension.Id)
            {
                _motifId = dimension.Id;
                _motif.sprite = Resources.Load<Sprite>(dimension.MotifKey.Value);
            }

            _motif.color = ink;
            _title.color = ink;
            _body.text = dimension.Lore;
            _body.color = ink;
            BookView.SetBorders(this, config);
        }

        private void OnStateChanged()
        {
            Render();
        }
    }
}
