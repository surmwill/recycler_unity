using UnityEngine;

namespace RecyclerScrollRect
{
   /// <summary>
   /// Demos appending entries to a Recycler. (The endcap will fetch and append more entries once we scroll to it)
   /// </summary>
   public class TestAppendPrependRecycler : TestRecycler<EmptyRecyclerData, string>
   {
      [SerializeField]
      private EmptyRecyclerScrollRect _appendRecycler = null;

      private const int InitEntries = 30;
      private const int NumPrependEntries = 10;

      protected override RecyclerScrollRect<EmptyRecyclerData, string> ValidateRecycler => _appendRecycler;
      
      protected override string DemoTitle { get; }
      
      protected override string DemoDescription { get; }
      
      protected override string[] DemoButtonDescriptions { get; }

      protected override void Start()
      {
         base.Start();
         _appendRecycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(InitEntries));
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
