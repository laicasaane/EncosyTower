using EncosyTower.PageFlows;
using EncosyTower.Samples.UguiPages.Attachments;

namespace EncosyTower.Samples.UguiPages.PageUserData
{
    /// <summary>Reads the page user data that a page gets in <see cref="PageContext.UserData"/>.</summary>
    internal static class PageUserDataAPI
    {
        public static bool TryGetPage(in PageContext context, out DimensionPageData data)
        {
            if (context.UserData.TryGetValue(out var value) && value is DimensionPageData page)
            {
                data = page;
                return true;
            }

            data = null;
            return false;
        }

        public static bool TryGetSet(in PageContext context, out AttachmentSet set)
        {
            if (context.UserData.TryGetValue(out var value) && value is AttachmentSet attachmentSet)
            {
                set = attachmentSet;
                return true;
            }

            set = null;
            return false;
        }
    }
}
