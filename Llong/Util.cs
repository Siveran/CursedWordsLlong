namespace Llong
{
    class Util
    {
        // Get items in Snapshot, Frankenstein, and Random Access Memory
        // Returns a list including all the original items plus the buried things
        public static List<Item> UnpackItems(List<Item> items)
        {
            List<Item> allItems = [.. items];
            Item ramItem = allItems.Find((Item item) => item is RandomAccessMemory);
            Item snapshotItem = allItems.Find((Item item) => item is Snapshot);
            List<Item> frankensteinItems = allItems.Where((Item item) => item is Frankenstein).ToList();
            Item iceCubeItem = allItems.Find((Item item) => item is IceCube);
            if (ramItem != null)
            {
                RandomAccessMemory randomAccessMemory = ramItem as RandomAccessMemory;
                allItems.AddRange(randomAccessMemory.ItemsInMemory);
            }
            if (snapshotItem != null)
            {
                Snapshot snapshot = snapshotItem as Snapshot;
                if (snapshot.SnapshottedItem != null)
                {
                    allItems.Add(snapshot.SnapshottedItem);
                }
            }
            foreach (Item frankensteinItem in frankensteinItems)
            {
                Frankenstein frankenstein = frankensteinItem as Frankenstein;
                allItems.AddRange(frankenstein.StitchedItems);
            }
            if (iceCubeItem != null)
            {
                IceCube iceCube = iceCubeItem as IceCube;
                if (iceCube.FrozenItem != null)
                {
                    allItems.Add(iceCube.FrozenItem);
                }
            }

            return allItems;
        }
    }
}
