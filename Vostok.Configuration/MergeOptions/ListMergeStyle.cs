namespace Vostok.Configuration.MergeOptions
{
    /// <summary>
    /// Specifies the way array nodes are merged.
    /// </summary>
    public enum ListMergeStyle
    {
        /// <summary>
        /// Replace one list with another.
        /// </summary>
        Replace,

        /// <summary>
        /// Produce a list containing elements from both lists. All elements from the first list, then all elements from the second, preserving order inside lists.
        /// </summary>
        Concat,

        /// <summary>
        /// Produce a list containing unique items from both lists. The order is the same as with <see cref="Concat"/>.
        /// </summary>
        Union
    }
}