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

namespace EncosyTower.Samples.UguiPages.Screens
{
    /// <summary>The core screen: it shows the six runes and casts the incantation.</summary>
    /// <remarks>A page of the Screen flow. It rejects the creation until the core is unlocked.</remarks>
    public class CoreScreen
        : UguiPageBehaviour<AppFlowScopes>
        , IPageOnCreateAsync
        , IPageOnAttachToFlowAsync
        , IPageOnReturnToPool
    {
        /// <summary>The background image of the screen.</summary>
        [Tooltip("The background image of the screen.")]
        [SerializeField] private Image _background;

        /// <summary>The seal image of the screen.</summary>
        [Tooltip("The seal image of the screen.")]
        [SerializeField] private Image _seal;

        /// <summary>The title of the core.</summary>
        [Tooltip("The title of the core.")]
        [SerializeField] private TMP_Text _title;

        /// <summary>The lore text of the core.</summary>
        [Tooltip("The lore text of the core.")]
        [SerializeField] private TMP_Text _lore;

        /// <summary>The text of the spell.</summary>
        [Tooltip("The text of the spell.")]
        [SerializeField] private TMP_Text _spell;

        /// <summary>The button that casts the incantation or reads the name.</summary>
        [Tooltip("The button that casts the incantation or reads the name.")]
        [SerializeField] private Button _cast;

        /// <summary>The rune views of the six runes.</summary>
        [Tooltip("The rune views of the six runes.")]
        [SerializeField] private RuneView[] _runes;

        /// <summary>The rect that holds the navigation bar page.</summary>
        [Tooltip("The rect that holds the navigation bar page.")]
        [SerializeField] private RectTransform _navigationBarHost;

        private AppState _state;
        private AttachmentSet _set;
        private bool _isRegistered;

        /// <inheritdoc/>
        /// <remarks>
        /// Rejects the creation until the core is unlocked.
        /// </remarks>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested
                || PageUserDataAPI.TryGetPage(in context, out var data) == false
                || data.State.CoreUnlocked == false
            )
            {
                return UnityTask.FromResult(false);
            }

            if (_isRegistered == false)
            {
                _isRegistered = true;
                _cast.onClick.AddListener(OnCastClicked);
            }

            _state = data.State;
            _state.Changed += OnStateChanged;

            Render();
            return UnityTask.FromResult(true);
        }

        /// <inheritdoc/>
        public async UnityTask<bool> OnAttachToFlowAsync(IPageFlow flow, PageContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            _set = await _state.Attachments.RentAsync(
                  location: BookLocation.Of(Dimension.Core)
                , navigationBarHost: _navigationBarHost
                , detailHost: null
                , cardsHost: null
                , token: token
            );

            return true;
        }

        /// <inheritdoc/>
        public void OnReturnToPool(PageContext context)
        {
            if (_state != null)
            {
                _state.Changed -= OnStateChanged;

                if (_set != null)
                {
                    _state.Attachments.Return(_set);
                    _set = null;
                }

                _state = null;
            }
        }

        private void OnDestroy()
        {
            if (_isRegistered)
            {
                _cast.onClick.RemoveListener(OnCastClicked);
            }
        }

        private void Render()
        {
            var config = _state.Config;

            var cast = _state.IncantationCast;
            var visited = _state.CoreVisited;
            var ink = config.Color(ConfigColorType.Ink);
            var navigationBar = config.Color(ConfigColorType.NavigationBar);
            var spellId = visited ? ConfigTextType.CoreSpellLearned : ConfigTextType.CoreSpellWaiting;
            var buttonId = cast ? ConfigTextType.CoreReadNameButton : ConfigTextType.CoreCastButton;

            _background.color = config.Color(DimensionCatalog.Core.Surface);
            _seal.color = ink;
            _title.text = DimensionCatalog.Core.Title;
            _title.color = ink;
            _lore.text = DimensionCatalog.Core.Lore;
            _lore.color = ink;
            _spell.text = config.Text(spellId);
            _spell.color = ink;

            _cast.gameObject.SetActive(visited);
            BookView.SetLabel(_cast, config.Text(buttonId));
            BookView.SetColors(_cast, navigationBar, ink, config);

            var interactive = _state.IsPopupActive == false;

            BookView.SetInteractable(_cast, interactive, config);

            for (var i = 0; i < RuneCatalog.COUNT; i++)
            {
                var rune = (Rune)i;
                var hasRune = cast || _state.TryGetAcquiredRune(i, out rune);

                if (hasRune == false)
                {
                    _runes[i].SetBlank();
                }
                else
                {
                    _runes[i].SetRune(rune, decoded: cast, number: i + 1);
                }

                _runes[i].SetInk(ink);
            }
        }

        private void OnStateChanged()
        {
            Render();
        }

        private void OnCastClicked()
        {
            var action = _state.IncantationCast ? AppAction.ReadName : AppAction.CastIncantation;
            BookActions.Publish(Publisher, FlowScopeCollection, action);
        }
    }
}
