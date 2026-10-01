namespace EncosyTower.SourceGen
{
    public enum UnmanagedSizeUpperBoundFailure : byte
    {
        None = 0,
        OpenGeneric = 1,
        ManagedStorage = 2,
        RefLikeType = 3,
        PointerStorage = 4,
        PointerSizedStorage = 5,
        AutoLayout = 6,
        UnsupportedPacking = 7,
        OpaqueExternalStorage = 8,
        GeneratedStorageUnknown = 9,
        InvalidGenericExplicitLayout = 10,
        InvalidLayout = 11,
        RecursiveLayout = 12,
        SizeOverflow = 13,
    }
}
