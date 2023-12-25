using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T.UI
{
    public class UIPopUp : UIMonoBehaviour
    {
        public override void Show()
        {
            UIManager.instance.Push(this);
            base.Show();
        }

        public override void Hide()
        {
            UIManager.instance.Pop(this);
            base.Hide();
        }

        protected override void OnOtherCanvasSelected(UIMonoBehaviour other)
        {
            if (other is UIPopUp)
                UIManager.instance.Push(other);

            base.OnOtherCanvasSelected(other);
        }
    }
}
