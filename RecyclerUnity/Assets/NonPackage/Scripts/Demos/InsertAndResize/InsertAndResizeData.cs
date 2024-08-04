using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Data for recycler entries used in our insert and resize demo.
    /// </summary>
    public class InsertAndResizeData : IRecyclerScrollRectData<string>
    {
        /// <summary>
        /// The key of the data.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Whether the entry should grow upon being bound.
        /// </summary>
        public bool ShouldGrow { get; }

        /// <summary>
        /// Whether the entry already grew.
        /// </summary>
        public bool DidGrow { get; set; }

        public InsertAndResizeData(bool shouldGrow)
        {
            ShouldGrow = shouldGrow;
            Key = Guid.NewGuid().ToString();
        }
    }
}
