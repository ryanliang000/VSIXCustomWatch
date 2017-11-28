using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace VSIXCutomWatch
{
    /// <summary>
    /// This is the class that implements the m_package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid m_package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This m_package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the m_package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this m_package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(CommandWatchPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    public sealed class CommandWatchPackage : Package
    {
        /// <summary>
        /// Command1Package GUID string.
        /// </summary>
        public const string PackageGuidString = "e3866edd-d694-4f0b-b3e9-e74fa44f1db8";

        /// <summary>
        /// Initializes a new instance of the <see cref="Command1"/> class.
        /// </summary>
        public CommandWatchPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the m_package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.

        }


        #region Package Members

        /// <summary>
        /// Initialization of the m_package; this method is called right after the m_package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            // 获取DTE
            // DTE2 dte = (DTE2)GetService(typeof(DTE));
            // DTE2 dte = (DTE2)Package.GetGlobalService(typeof(DTE));
            // string strNewExpr = string.Format("{{,,{0}}}{1}({2}, \"{3}\")", "1", "2", "3", "4");

            // 初始化命令
            CommandCustomWatch.Initialize(this);
            base.Initialize();
        }

        #endregion

        public void OnEnterBreak(dbgEventReason Reason, ref dbgExecutionAction ExecutionAction)
        {
            //CommandCustomWatch.Instance.MenuEnabled = true;
            System.Windows.Forms.MessageBox.Show("Events are attached.");
        }
        public void OnEnterOther(dbgEventReason Reason)
        {
            //CommandCustomWatch.Instance.MenuEnabled = false;
            System.Windows.Forms.MessageBox.Show("Events are attached.");
        }

        public void OnContextChanged(EnvDTE.Process NewProcess, Program NewProgram, Thread NewThread, EnvDTE.StackFrame NewStackFrame)
        {
            //CommandCustomWatch.Instance.MenuEnabled = false;
            System.Windows.Forms.MessageBox.Show("Events are attached.");
        }
    }

}
