using System;
using System.Drawing;

namespace Kuat
{
    public class KuatControl : IKuatObject
    {
        public Point Location { get; set; }
        public Size Size { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public Color ForeColor { get; set; }
        public Color BackColor { get; set; }
        public int ZIndex { get; set; }
        public int TabIndex { get; set; }
        public bool TabStop { get; set; }

        /// <inheritdoc />
        public void Update(double dt)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Render(double partialTicks)
        {
            throw new NotImplementedException();
        }
    }
}