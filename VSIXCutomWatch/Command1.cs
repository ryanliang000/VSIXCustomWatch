using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Windows.Forms;


namespace VSIXCutomWatch
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CommandCustomWatch
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;
        public const int CommandIdConfig = 0x0101;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("2a6274cd-2be9-4f8d-93cb-0216e16783bc");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package m_package;

        public MenuCommand m_menuCommand = null;
        public MenuCommand m_menuCommandConfig = null;
        public DTE m_dte = null;
        public ProcEvent m_procEvent = null;
        OutputWindowPane m_watchPane = null;


        public bool MenuEnabled
        {
            get { return m_menuCommand.Enabled; }
            set { m_menuCommand.Enabled = value; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CommandCustomWatch"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner m_package, not null.</param>
        private CommandCustomWatch(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("m_package");
            }

            m_package = package;
            m_dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                // command watch
                var menuCommandID = new CommandID(CommandSet, CommandId);
                m_menuCommand = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(m_menuCommand);

                // command config
                var menuCommandIDConfig = new CommandID(CommandSet, CommandIdConfig);
                m_menuCommandConfig = new MenuCommand(this.MenuItemCallbackConfig, menuCommandIDConfig);
                commandService.AddCommand(m_menuCommandConfig);
            }


            m_procEvent = new ProcEvent(this);
        }


        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CommandCustomWatch Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner m_package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.m_package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner m_package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new CommandCustomWatch(package);
        }

        public void OutputStr(string str)
        {
            if (m_dte == null)
                return;
            if (m_dte.Windows == null)
                return;

            Window wind = (Window)m_dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
            wind.Visible = true;
            if (m_watchPane == null)
            {
                OutputWindow outputWind = (OutputWindow)wind.Object;
                m_watchPane = outputWind.OutputWindowPanes.Add("Custom Watch");
            }
            m_watchPane.Activate();
            m_watchPane.OutputString(str + "\n");
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            //string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            //string title = "Custom Watch";
            //// Show a message box to prove we were here
            //VsShellUtilities.ShowMessageBox(
            //    this.ServiceProvider,
            //    message,
            //    title,
            //    OLEMSGICON.OLEMSGICON_INFO,
            //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            if (m_dte == null)
                return;
            Document doc = m_dte.ActiveDocument;
            if (doc == null)
                return;
            if (doc.Type != "Text")
                return;

            //select text
            TextSelection selection = (TextSelection)doc.Selection;
            string strSelectText = selection.Text;
            strSelectText.Trim();

            // invalid selection
            if (strSelectText.Length == 0)
            {
                EditPoint pt = (EditPoint)selection.ActivePoint.CreateEditPoint();
                EditPoint ptLeft = pt.CreateEditPoint();
                EditPoint ptRight = pt.CreateEditPoint();
                for (ptLeft.CharLeft(); !ptLeft.AtStartOfLine; ptLeft.CharLeft())
                {
                    if (!IsValidCharOfName(ptLeft.GetText(pt)[0]))
                    {
                        break;
                    }
                }
                ptLeft.CharRight();
                for (ptRight.CharRight(); !ptRight.AtEndOfLine; ptRight.CharRight())
                {
                    var strText = ptRight.GetText(pt);
                    if (!IsValidCharOfName(strText[strText.Length - 1]))
                    {
                        break;
                    }
                }
                ptRight.CharLeft();
                strSelectText = ptLeft.GetText(ptRight);
                if (!IsValidStartCharOfName(strSelectText[0]))
                   return;
            }
            if (strSelectText.Length == 0)
                return;

            if (m_watchPane != null)
            {
                m_watchPane.Clear();
            }

            string strRetValue = "";
            string strErrorMsg = "";
            if (m_procEvent.CalcExpression(strSelectText, out strErrorMsg, out strRetValue))
            {
                if (strRetValue != "" && strRetValue.IndexOf("Error") == -1 && strRetValue.IndexOf("error") == -1)
                {
                    Clipboard.SetText(strRetValue);
                    OutputStr("(Inform: Already copy the data below to clipborad)");
                }
                OutputStr(strRetValue);
            }
            else
            {
                OutputStr("Error Occur: " + strErrorMsg + "\n");
            }
        }

        private bool IsValidCharOfName(char ch)
        {
            if ((ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || ch == '_')
            {
                return true;
            }
            return false;
        }
        private bool IsValidStartCharOfName(char ch)
        {
            if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || ch == '_')
            {
                return true;
            }
            return false;
        }

        private void MenuItemCallbackConfig(object sender, EventArgs e)
        {
            //string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            //string title = "Custom Watch";
            //// Show a message box to prove we were here
            //VsShellUtilities.ShowMessageBox(
            //    this.ServiceProvider,
            //    message,
            //    title,
            //    OLEMSGICON.OLEMSGICON_INFO,
            //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            ;

            configForm form = new configForm();
            form.ShowDialog();
        }
    }
}
