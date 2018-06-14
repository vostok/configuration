namespace Vostok.Configuration.MergeOptions
{
    public class SettingsMergeOptions
    {
        // CR(krait): Let's just put the default values into property initializers.
        public static SettingsMergeOptions Default() =>
            new SettingsMergeOptions {TreeMergeStyle = TreeMergeStyle.Shallow, ListMergeStyle = ListMergeStyle.Concat};

        public TreeMergeStyle TreeMergeStyle { get; set; }
        public ListMergeStyle ListMergeStyle { get; set; }
    }
}