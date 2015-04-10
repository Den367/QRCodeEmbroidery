using System.IO;


namespace EmbroideryFile
{

    public interface IGetEmbroideryData
    {

        EmbroideryData Design { get; }
        void LoadEmbroidery(Stream stream);
    }
}
