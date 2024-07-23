using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RecyclerScrollRect
{
   /// <summary>
   /// Demos appending entries to a Recycler. (The endcap will fetch and append more entries once we scroll to it)
   /// </summary>
   public class TestAppendRecycler : MonoBehaviour
   {
      [SerializeField]
      private EmptyRecyclerScrollRect _appendRecycler = null;

      private const int InitEntries = 30;
      private const int NumPrependEntries = 10;

      private void Start()
      {
         _appendRecycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(InitEntries));
      }

      private void Update()
      {
         if (Input.GetKeyDown(KeyCode.A))
         {
            _appendRecycler.PrependEntries(EmptyRecyclerData.GenerateEmptyData(NumPrependEntries));
         }
      }
   }
}
