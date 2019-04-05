using System.Collections.Generic;
using Kuat.Control;

namespace Kuat
{
    public class KuatControlCollection : List<KuatControl>
    {
        private readonly KuatControl _owner;

        public KuatControlCollection(KuatControl owner)
        {
            _owner = owner;
        }

        public new void Add(KuatControl item)
        {
            if (item == null)
                return;

            base.Add(item);
            item.Parent?.Controls.Remove(item);
            item.Parent = _owner;
        }
    }
}