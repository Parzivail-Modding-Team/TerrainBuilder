using NanoVGDotNet.NanoVG;

namespace Kuat.Control
{
    public class KuatRadioButton : KuatControl
    {
        public bool Checked { get; set; }

        /// <inheritdoc />
        public KuatRadioButton(string name) : base(name)
        {
        }

        /// <inheritdoc />
        protected override void OnPaint(object sender, NvgContext e)
        {
            base.OnPaint(sender, e);

            e.BeginPath();
            e.Circle(ClientLocation.X + 7, ClientLocation.Y + 7, 7);

            e.FillColor(Checked ? NanoVg.Rgba(ActiveColor) : NanoVg.Rgba(0xFF475054));
            e.Fill();

            e.StrokeColor(NanoVg.Rgba(0xFF192025));
            e.Stroke();

            e.FillColor(NanoVg.Rgba(ForeColor));
            e.FontFace(Font.Family);
            e.FontSize(Font.Size);
            e.TextAlign(NvgAlign.Left | NvgAlign.Middle);
            e.Text(ClientLocation.X + 18, ClientLocation.Y + 7.5f, Text);
        }

        /// <inheritdoc />
        protected override void OnClick(object sender, MouseClickEventArgs e)
        {
            base.OnClick(sender, e);
            if (Checked) return;
            UncheckNeighbors();
            Checked = true;
        }

        private void UncheckNeighbors()
        {
            foreach (var control in Parent.Controls)
                if (control is KuatRadioButton radioButton)
                    radioButton.Checked = false;
        }
    }
}