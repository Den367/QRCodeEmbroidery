namespace EmbroideryFile
{
    public struct Line
    {
        public Coords Dot1 { get; set; }
        public Coords Dot2 { get; set; }
        public int Length { get; set; }
        public bool Lowest { get; set; }
    }
}
