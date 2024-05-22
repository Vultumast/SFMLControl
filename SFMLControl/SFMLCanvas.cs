using System.ComponentModel;

namespace SFMLControl
{
    public partial class SFMLCanvas : UserControl
    {
        public SFMLCanvas()
        {
            InitializeComponent();
        }

        #region Internal

        // This is taken from GLControl
        // https://github.com/opentk/GLControl/blob/4796deca94ff644cf0b345d103d85cef756b49d1/OpenTK.WinForms/GLControl.cs#L447

        /// <summary>
        /// A fix for the badly-broken DesignMode property, this answers (somewhat more
        /// reliably) whether this is DesignMode or not.  This does *not* work when invoked
        /// from the GLControl's constructor.
        /// </summary>
        /// <returns>True if this is in design mode, false if it is not.</returns>
        private bool DetermineIfThisIsInDesignMode()
        {
            // The obvious test.
            if (DesignMode)
                return true;

            // This works on .NET Framework but no longer seems to work reliably on .NET Core.
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return true;

            // Try walking the control tree to see if any ancestors are in DesignMode.
            for (Control control = this; control != null; control = control.Parent!)
            {
                if (control.Site != null && control.Site.DesignMode)
                    return true;
            }

            // Try checking for `IDesignerHost` in the service collection.
            if (GetService(typeof(System.ComponentModel.Design.IDesignerHost)) != null)
                return true;

            // Last-ditch attempt:  Is the process named `devenv` or `VisualStudio`?
            // These are bad, hacky tests, but they *can* work sometimes.
            if (System.Reflection.Assembly.GetExecutingAssembly().Location.Contains("VisualStudio", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(System.Diagnostics.Process.GetCurrentProcess().ProcessName, "devenv", StringComparison.OrdinalIgnoreCase))
                return true;

            // Nope.  Not design mode.  Probably.  Maybe.
            return false;
        }
        #endregion

        #region Events
        public event EventHandler<SFMLRenderBaseEvents>? RenderStart = null;
        public event EventHandler<SFMLRenderBaseEvents>? RenderEnd = null;
        public event EventHandler<SFMLRenderBaseEvents>? Draw = null;

        #endregion

        #region Private Fields
        private bool _isDesignMode = false;
        private volatile bool _renderLocked = false;

        #endregion

        #region Properties
        public bool IsDesignMode => _isDesignMode;

        public bool RenderLocked => _renderLocked;

        public bool IsRenderStarted { get; private set; } = false;


        #endregion

        #region Private Functions
        protected override void OnHandleDestroyed(EventArgs e)
        {
            sfmlBackgroundWorker.CancelAsync();
            base.OnHandleDestroyed(e);
        }

        private void SFMLCanvas_Load(object sender, EventArgs e)
        {
            _isDesignMode = DetermineIfThisIsInDesignMode();
        }

        private void SfmlBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (e.Argument is null)
                return;

            RenderLoopArguments args = (RenderLoopArguments)e.Argument;

            SFML.Graphics.RenderWindow renderWindow = new (args.HWND);


            RenderStart?.Invoke(sfmlBackgroundWorker, new(renderWindow));
            while (renderWindow.IsOpen && !sfmlBackgroundWorker.CancellationPending)
            {
                _renderLocked = true;
                Draw?.Invoke(sfmlBackgroundWorker, new(renderWindow));
                _renderLocked = false;
            }
            RenderEnd?.Invoke(sfmlBackgroundWorker, new(renderWindow));
        }
        #endregion

        #region Public Functions

        public void StartRender()
        {
            if (IsRenderStarted || IsDesignMode)
                return;


            sfmlBackgroundWorker.RunWorkerAsync(new RenderLoopArguments(Handle));
            IsRenderStarted = true;
        }
        public void StopRender()
        {
            IsRenderStarted = false;
            sfmlBackgroundWorker.CancelAsync();
        }

        #endregion

    }
}
