using System;

namespace Swill.Recycler
{
   /// <summary>
   /// Data used to demo scrolling to an index in a recycler
   /// </summary>
   public class ScrollToIndexData : IRecyclerScrollRectData<string>
   {
      public string Key { get; }

      public ScrollToIndexData()
      {
         Key = Guid.NewGuid().ToString();
      }
   }
}
