#if UNITY_UGUI

using EncosyTower.Pooling;
using UnityEngine;

namespace EncosyTower.PageFlows.UguiPages
{
    /// <summary>The pooling and transition options of one uGUI page.</summary>
    /// <remarks>
    /// Put it on the root GameObject of the page asset; <see cref="UguiPageBehaviour"/> adds it when missing.
    /// </remarks>
    public class UguiPageOptions : MonoBehaviour
    {
        /// <summary>How the pool rents an instance of this page.</summary>
        [Tooltip("How the pool rents an instance of this page.")]
        [SerializeField] private RentingStrategy _rentingStrategy;

        /// <summary>How the pool takes an instance of this page back.</summary>
        [Tooltip("How the pool takes an instance of this page back.")]
        [SerializeField] private ReturningStrategy _returningStrategy;

        /// <summary>The options a flow reads when it shows or hides this page.</summary>
        [Tooltip("The options a flow reads when it shows or hides this page.")]
        [SerializeField] private PageOptions _pageOptions;

        /// <summary>Gets or sets how the pool rents an instance of this page.</summary>
        public RentingStrategy RentingStrategy
        {
            get => _rentingStrategy;
            set => _rentingStrategy = value;
        }

        /// <summary>Gets or sets how the pool takes an instance of this page back.</summary>
        public ReturningStrategy ReturningStrategy
        {
            get => _returningStrategy;
            set => _returningStrategy = value;
        }

        /// <summary>Gets or sets the options a flow reads when it shows or hides this page.</summary>
        public PageOptions PageOptions
        {
            get => _pageOptions;
            set => _pageOptions = value;
        }
    }
}

#endif
