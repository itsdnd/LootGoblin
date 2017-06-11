using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LootGoblin
{
    public static class UIElementHelper
    {
        // Thanks to user WPFGermany on StackOverflow at https://stackoverflow.com/a/41136328 
        public static ScrollViewer GetScrollViewer(UIElement element)
        {
            if (element == null) return null;

            ScrollViewer scrollViewer = null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element) && scrollViewer == null; i++)
            {
                if (VisualTreeHelper.GetChild(element, i) is ScrollViewer)
                {
                    scrollViewer = (ScrollViewer)(VisualTreeHelper.GetChild(element, i));
                }
                else
                {
                    scrollViewer = GetScrollViewer(VisualTreeHelper.GetChild(element, i) as UIElement);
                }
            }
            return scrollViewer;
        }
    }
}
