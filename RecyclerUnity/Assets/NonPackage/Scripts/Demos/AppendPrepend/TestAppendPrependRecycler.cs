using UnityEngine;

namespace RecyclerScrollRect
{
   /// <summary>
   /// Demos appending and prepending entries to a Recycler.
   /// (The endcap will fetch and append more entries once we scroll to it.)
   /// </summary>
   public class TestAppendPrependRecycler : TestRecycler<EmptyRecyclerData, string>
   {
      [SerializeField]
      private EmptyRecyclerScrollRect _appendRecycler = null;

      private const int InitEntries = 30;
      private const int NumPrependEntries = 10;

      protected override RecyclerScrollRect<EmptyRecyclerData, string> ValidateRecycler => _appendRecycler;

      protected override string DemoTitle => "Append and Prepend demo";
      
      protected override string DemoDescription => "Demos appending and prepending entries to a Recycler.\n" +
                                                   "(The endcap will fetch and append more entries once we scroll to it.)";

      protected override string[] DemoButtonDescriptions => new [] { "0: Prepends entries." };

      protected override void Start()
      {
         base.Start();
         _appendRecycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(InitEntries));
      }

      private void Update()
      {
         if (Input.GetKeyDown(KeyCode.A) || DemoToolbar.GetButtonDown(0))
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
