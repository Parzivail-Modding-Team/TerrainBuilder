using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuat
{
    public class KuatWindow : IKuatObject
    {
        public ConcurrentBag<KuatControl> Widgets { get; }

        public KuatWindow()
        {
            Widgets = new ConcurrentBag<KuatControl>();
        }

        /// <inheritdoc />
        public void Update(double dt)
        {
            foreach (var widget in Widgets)
                widget.Update(dt);
        }

        /// <inheritdoc />
        public void Render(double partialTicks)
        {
            foreach (var widget in Widgets.OrderBy(control => control.ZIndex))
                widget.Render(partialTicks);
        }
    }
}
