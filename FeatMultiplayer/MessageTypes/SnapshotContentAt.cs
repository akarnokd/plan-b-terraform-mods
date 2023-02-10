using System.IO;

namespace FeatMultiplayer
{
    /// <summary>
    /// Contains the contentId and contentData of a particular coordinate.
    /// </summary>
    internal class SnapshotContentAt
    {
        internal int2 coords;
        internal byte contentId;
        internal uint contentData;

        internal void GetSnapshot(int2 coords)
        {
            this.coords = coords;
            contentId = GHexes.contentId[coords.x, coords.y];
            contentData = GHexes.contentData[coords.x, coords.y];
        }

        internal void ApplySnapshot()
        {
            GHexes.contentId[coords.x, coords.y] = contentId;
            GHexes.contentData[coords.x, coords.y] = contentData;
        }

        internal void Decode(BinaryReader input)
        {
            coords = new int2(input.ReadInt32(), input.ReadInt32());
            contentId = input.ReadByte();
            contentData = input.ReadUInt32();
        }

        internal void Encode(BinaryWriter output)
        {
            output.Write(coords.x);
            output.Write(coords.y);
            output.Write(contentId);
            output.Write(contentData);
        }
    }
}
