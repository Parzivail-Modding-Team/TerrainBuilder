namespace Kuat
{
    public class KuatFont
    {
        public readonly string Family;
        public readonly float Size;

        public KuatFont() : this("sans", 12)
        {
        }

        public KuatFont(string family, float size)
        {
            Family = family;
            Size = size;
        }
    }
}