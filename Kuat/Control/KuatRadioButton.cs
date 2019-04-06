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
            e.Circle(ClientRectangle.Location.X + 7, ClientRectangle.Location.Y + 7, 7);

            e.FillColor(Checked ? NanoVg.Rgba(ActiveColor) :
                Hover ? NanoVg.Rgba(HoverColor) : NanoVg.Rgba(BackColor));
            e.Fill();

            e.StrokeColor(NanoVg.Rgba(43, 51, 55, 255));
            e.Stroke();

            e.FillColor(NanoVg.Rgba(ForeColor));
            e.FontFace(Font.Family);
            e.FontSize(Font.Size);
            e.TextAlign(NvgAlign.Left | NvgAlign.Middle);
            e.Text(ClientRectangle.Location.X + 18, ClientRectangle.Location.Y + 7.5f, Text);
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
	        {
		        if (control is KuatRadioButton radioButton)
			        radioButton.Checked = false;
	        }
        }
    }
}