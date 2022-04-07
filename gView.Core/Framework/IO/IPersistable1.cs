namespace gView.Framework.IO
{
    public interface IPersistable 
	{
        void Load(IPersistStream stream);
		void Save(IPersistStream stream);
	}
}
