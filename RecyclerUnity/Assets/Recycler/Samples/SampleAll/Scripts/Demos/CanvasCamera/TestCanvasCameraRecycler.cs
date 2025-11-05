using UnityEngine;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Tests that our recycler works with a Screen Space - Camera canvas
    /// </summary>
    public class TestCanvasCameraRecycler : TestRecycler<string, EmptyRecyclerData>
    {
        [SerializeField]
        private EmptyRecyclerScrollRect _recycler = null;

        private const int InitNumEntries = 30;

        protected override RecyclerScrollRect<string, EmptyRecyclerData> ValidateRecycler => _recycler;

        protected override string DemoTitle => "Screen Space - Camera Canvas Demo";

        protected override string DemoDescription => "Tests if the recycler stays stable with a moving camera and a Screen Space - Camera canvas.";

        protected override string[] DemoButtonDescriptions => null;

        protected override void Start()
        {
            base.Start();
            _recycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(InitNumEntries));
        }

        private void OnValidate()
        {
            if (_recycler == null)
            {
                _recycler = GetComponent<EmptyRecyclerScrollRect>();
            }
        }
    }
}
