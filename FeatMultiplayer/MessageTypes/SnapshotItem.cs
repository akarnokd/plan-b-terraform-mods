namespace FeatMultiplayer
{
    internal class SnapshotItem
    {
        internal string codeName;
        internal int count;

        internal void GetSnapshot(CItem item)
        {
            codeName = item.codeName;
            count = item.nbOwned;
        }
    }
}
