namespace Vostok.Configuration.Abstractions.MergeOptions
{
    public class SettingsMergeOptions
    {
        public ObjectMergeStyle ObjectMergeStyle { get; set; } = ObjectMergeStyle.Deep;

        public ArrayMergeStyle ArrayMergeStyle { get; set; } = ArrayMergeStyle.Concat;
    }
}