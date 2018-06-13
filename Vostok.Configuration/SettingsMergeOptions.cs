namespace Vostok.Configuration
{
    public class SettingsMergeOptions
    {
        public static SettingsMergeOptions Default() =>
            new SettingsMergeOptions {TreeMergeStyle = TreeMergeStyle.Shallow, ListMergeStyle = ListMergeStyle.Concat};

        public TreeMergeStyle TreeMergeStyle { get; set; }
        public ListMergeStyle ListMergeStyle { get; set; }
    }
}