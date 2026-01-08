using System.Reflection;
using System.Windows.Forms;

namespace FashionShop.Presentation
{
    public class DoubleBufferedListView : ListView
    {
        public DoubleBufferedListView()
        {
            // bật double buffer để đỡ giật và hạn chế lỗi vẽ
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(this, true, null);
        }
    }
}
