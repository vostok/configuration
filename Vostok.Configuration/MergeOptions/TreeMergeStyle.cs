namespace Vostok.Configuration.MergeOptions
{
    /// <summary>
    /// Specifies the way settings trees are merged.
    /// </summary>
    public enum TreeMergeStyle
    {
        /// <summary>
        /// If the other tree node has another configuration of children, union the children from both nodes. Then merge the children with same names recursively.
        /// </summary>
        Deep,

        /// <summary>
        /// If the other tree node has another configuration of children, just replace it with the current node. Elsewise, merge all the children recursively.
        /// </summary>
        Shallow
    }
}