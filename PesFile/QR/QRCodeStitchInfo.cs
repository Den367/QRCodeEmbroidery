

namespace EmbroideryFile
{
    public  class QRCodeStitchInfo
    {
        
        public string DesignName { get; set; }
        public int dX {get;set;}
        public int dY {get;set;}
        public int cellSize { get; set; }
        public string QrCodeText { get; set; }
        public bool[][] Matrix { get; set; }
        public int Dimension { get { if (Matrix != null) return Matrix.GetUpperBound(0) + 1; else return 0; } }

        public QRCodeStitchInfo()
        {
            dX = 25; dY = 2; cellSize = 25; DesignName = "QR_CODE";
        }
    }
}
