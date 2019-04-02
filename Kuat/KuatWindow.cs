using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuat
{
    public class KuatWindow
    {
        public ConcurrentBag<KuatControl> Widgets { get; }

        public KuatWindow()
        {
            Widgets = new ConcurrentBag<KuatControl>();
        }
    }
}
