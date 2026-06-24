using System.Collections.ObjectModel;

namespace Memory_Storage;

public sealed class MemoryRecordGroup : ObservableCollection<MemoryRecord>
{
    public MemoryRecordGroup(string title, IEnumerable<MemoryRecord> records)
        : base(records)
    {
        Title = title;
    }

    public string Title { get; }
}
