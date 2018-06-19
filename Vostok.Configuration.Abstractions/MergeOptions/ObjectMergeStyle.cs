namespace Vostok.Configuration.Abstractions.MergeOptions
{
    /// <summary>
    /// Specifies the way object nodes are merged.
    /// </summary>
    public enum ObjectMergeStyle
    {
        /// <summary>
        /// Union the children from both nodes. Then merge children with same names recursively.
        /// </summary>
        Deep,

        /// <summary>
        /// Compare children of both nodes by names. If the sets of names match, regardless of order, then merge the pairs of matching children recursively. Elsewise, just replace the current node with the other node.
        /// </summary>
        Shallow
    }
}