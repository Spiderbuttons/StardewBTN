using System.Linq;
using StardewModdingAPI;

namespace SpiderCore.Common.Extensions
{
    public static class IInputHelperExtensions
    {
        public static bool IsAnyDown(this IInputHelper helper, params SButton[] buttons)
        {
            return buttons.Any(helper.IsDown);
        }
    }
}