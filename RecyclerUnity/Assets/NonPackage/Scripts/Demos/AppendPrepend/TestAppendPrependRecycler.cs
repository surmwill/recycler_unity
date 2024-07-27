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
   public class TestAppendPrependRecycler : MonoBehaviour
   {
      [SerializeField]
      private EmptyRecyclerScrollRect _appendRecycler = null;

      private const int InitEntries = 30;
      private const int NumPrependEntries = 10;

      private RecyclerValidityChecker<EmptyRecyclerData, string> _validityChecker;

      private void Start()
      {
         _validityChecker = new RecyclerValidityChecker<EmptyRecyclerData, string>(_appendRecycler);
         _validityChecker.Bind();
         
         _appendRecycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(InitEntries));
      }

      private void OnDestroy()
      {
         _validityChecker.Unbind();
      }

      private void Update()
      {
         if (Input.GetKeyDown(KeyCode.A))
         {
            _appendRecycler.PrependEntries(EmptyRecyclerData.GenerateEmptyData(NumPrependEntries));
         }
      }

      private void OnValidate()
      {
         if (_appendRecycler == null)
         {
            _appendRecycler = GetComponent<EmptyRecyclerScrollRect>();
         }
      }
   }
}
