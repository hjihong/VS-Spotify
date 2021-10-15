using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Task = System.Threading.Tasks.Task;

namespace VSSpotify
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SpotifyPlayPreviousCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4129;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("286dbb7b-e424-49b4-9f59-e1fba7654ca3");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly VSSpotifyPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpotifyPlayPreviousCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private SpotifyPlayPreviousCommand(VSSpotifyPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static SpotifyPlayPreviousCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(VSSpotifyPackage package)
        {
            // Switch to the main thread - the call to AddCommand in SpotifyPlayPreviousCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SpotifyPlayPreviousCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            this.package.SpotifyPlayerControl.PlayPreviousAsync().FileAndForget("vs/spotify/playpreviousfailure");
        }
    }
}
