namespace Kuat
{
    public interface IKuatObject
    {
        void Update(double dt);
        void Render(double partialTicks);
    }
}