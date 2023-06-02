// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

namespace FeatMultiplayer
{
    internal class SnapshotItem
    {
        internal string codeName;
        internal int count;
        internal int max;

        internal void GetSnapshot(CItem item)
        {
            codeName = item.codeName;
            count = item.nbOwned;
            max = item.nbOwnedMax;
        }
    }
}
