using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.Samples.UguiPages.App;
using EncosyTower.Samples.UguiPages.Attachments;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.Messages;
using EncosyTower.Samples.UguiPages.PageUserData;
using EncosyTower.Samples.UguiPages.State;
using EncosyTower.Samples.UguiPages.Views;
using EncosyTower.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.UguiPages.Cards
{
    /// <summary>The in-practice card of a dimension screen.</summary>
    /// <remarks>
    /// A page of the Cards flow, a multi-page list attached to the screen. The player can show it with the other card.
    /// </remarks>
    public class InPracticeCard : UguiPageBehaviour<AppFlowScopes>, IPageOnCreateAsync, IPageOnReturnToPool
    {
        /// <summary>The panel of the card.</summary>
        [Tooltip("The panel of the card.")]
        [SerializeField] private RectTransform _panel;

        /// <summary>The background image of the card.</summary>
        [Tooltip("The background image of the card.")]
        [SerializeField] private Image _background;

        /// <summary>The title of the card.</summary>
        [Tooltip("The title of the card.")]
        [SerializeField] private TMP_Text _title;

        /// <summary>The body text of the card.</summary>
        [Tooltip("The body text of the card.")]
        [SerializeField] private TMP_Text _body;

        /// <summary>The note text of the card.</summary>
        [Tooltip("The note text of the card.")]
        [SerializeField] private TMP_Text _note;

        /// <summary>The button that hides the card.</summary>
        [Tooltip("The button that hides the card.")]
        [SerializeField] private Button _close;

        private AttachmentSet _set;
        private AppState _state;
        private Dimension? _noteId;
        private string _noteText;
        private bool _isRegistered;

        /// <inheritdoc/>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || PageUserDataAPI.TryGetSet(in context, out var set) == false)
            {
                return UnityTask.FromResult(false);
            }

            if (_isRegistered == false)
            {
                _isRegistered = true;
                _close.onClick.AddListener(OnCloseClicked);
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

            if (_isRegistered)
            {
                _close.onClick.RemoveListener(OnCloseClicked);
            }
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

            var view = _state.ViewOf(dimension.Id);

            var config = _state.Config;
            var tones = ToneMath.Analogous(config, config.Color(dimension.Surface));
            var ink = config.Color(ConfigColorType.Ink);
            var splitFraction = config.Amount(ConfigAmountType.CardSplitFraction);
            var gap = view.ConceptVisible ? config.Amount(ConfigAmountType.CardGap) : 0f;

            _panel.anchorMin = new Vector2(view.ConceptVisible ? splitFraction : 0f, 0f);
            _panel.anchorMax = Vector2.one;
            _panel.offsetMin = _panel.offsetMax = Vector2.zero;
            _panel.offsetMin = new Vector2(gap, 0f);
            _background.color = tones.ComplementB;
            _title.text = config.Text(ConfigTextType.CardInPracticeTitle);
            _title.color = ink;
            _body.text = dimension.Practice;
            _body.color = ink;

            if (_noteId != dimension.Id)
            {
                _noteId = dimension.Id;
                _noteText = string.Format(config.Text(ConfigTextType.CardWatchFor), dimension.Watch);
            }

            _note.text = _noteText;
            _note.color = ink;
            BookView.SetBorders(this, config);
            BookView.SetColors(_close, tones.Button, ink, config);
            BookView.SetDimensionLooks(_close, tones.Button, tones.Button, ink, config);
            BookView.SetInteractable(_close, _state.IsPopupActive == false, config);
        }

        private void OnStateChanged()
        {
            Render();
        }

        private void OnCloseClicked()
        {
            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.CloseCard, 1);
        }
    }
}
