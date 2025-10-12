using UnityEngine;

namespace RecyclerScrollRect
{
   /// <summary>
   /// Demos appending and prepending entries to a Recycler.
   /// The endcap will fetch and append more entries once we scroll to it.
   /// </summary>
   public class TestAppendPrependRecycler : TestRecycler<EmptyRecyclerData, string>
   {
      [SerializeField]
      private EmptyRecyclerScrollRect _appendRecycler = null;

      public const int NumAppendPrependEntries = 10;
      
      private const int InitEntries = 30;

      protected override RecyclerScrollRect<EmptyRecyclerData, string> ValidateRecycler => _appendRecycler;

      protected override string DemoTitle => "Append and Prepend Demo";
      
      protected override string DemoDescription => $"Tests appending or prepending {NumAppendPrependEntries} entries to the Recycler. " +
                                                   "The endcap appends more entries, whereas the button prepends. " +
                                                   "Appended or prepended entries do not shift the visible window.";

      protected override string[] DemoButtonDescriptions => new [] { "0 (or 'A'): Prepends entries." };

      protected override void Start()
      {
         base.Start();
         _appendRecycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(InitEntries));
      }

      private void Update()
      {
         if (Input.GetKeyDown(KeyCode.A) || DemoToolbar.GetButtonDown(0))
         {
            _appendRecycler.PrependEntries(EmptyRecyclerData.GenerateEmptyData(NumAppendPrependEntries));
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
