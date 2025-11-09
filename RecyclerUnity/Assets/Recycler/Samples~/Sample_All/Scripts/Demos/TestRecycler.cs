using UnityEngine;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Base class for testing recyclers in demo scenes.
    /// </summary>
    public abstract class TestRecycler<TKeyEntryData, TEntryData> : MonoBehaviour where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        [SerializeField]
        private DemoToolbar _demoToolbar = null;

        /// <summary>
        /// The recycler we are testing.
        /// (Note: it would be great to serialize the recycler directly in this class, but we cannot serialize generic components.)
        /// </summary>
        protected abstract RecyclerScrollRect<TKeyEntryData, TEntryData> ValidateRecycler { get; }

        /// <summary>
        /// The toolbar for the demo.
        /// </summary>
        protected DemoToolbar DemoToolbar => _demoToolbar;
        
        /// <summary>
        /// The name of the demo.
        /// </summary>
        protected abstract string DemoTitle { get; }
        
        /// <summary>
        /// A description of the demo.
        /// </summary>
        protected abstract string DemoDescription { get; }
        
        /// <summary>
        /// A description of what each button does in the demo.
        /// </summary>
        protected abstract string[] DemoButtonDescriptions { get; }

        private RecyclerValidityChecker<TKeyEntryData, TEntryData> _validityChecker;

        protected virtual void Start()
        {
            _validityChecker = new RecyclerValidityChecker<TKeyEntryData, TEntryData>(ValidateRecycler);
            _validityChecker.Bind();
            
            DemoToolbar.SetHelpMenuDemoTitle(DemoTitle);
            DemoToolbar.SetHelpMenuDemoDescription(DemoDescription);
            DemoToolbar.SetHelpMenuDemoButtonDescriptions(DemoButtonDescriptions);
        }

        protected virtual void OnDestroy()
        {
            _validityChecker.Unbind();
        }
    }
}
