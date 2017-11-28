using EnvDTE;
using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.CallStack;
using System;

namespace VSIXCutomWatch
{

    class ProcEvent
    {
        protected EnvDTE.DebuggerEvents m_debugEvents = null;
        protected CommandCustomWatch m_watch = null;
        public const int s_nMaxReadBufLength = 2048000;
        public const int s_nTimeOutMiliSecond = 2500;


        public ProcEvent(CommandCustomWatch watch) 
        {
            m_watch = watch;
            m_debugEvents = watch.m_dte.Events.DebuggerEvents;
            m_debugEvents.OnEnterBreakMode += OnEnterBreak;
            m_debugEvents.OnEnterDesignMode += OnEnterOther;
            m_debugEvents.OnEnterRunMode += OnEnterOther;

            WatchConfig.InitAppConfig();
        }

        public void OnEnterBreak(dbgEventReason Reason, ref dbgExecutionAction ExecutionAction)
        {
            m_watch.MenuEnabled = true;
        }
        public void OnEnterOther(dbgEventReason Reason)
        {
            m_watch.MenuEnabled = false;
        }


        public bool CalcExpression(string var, out string strErrorMsg, out string strRetValue)
        {
            strRetValue = "";
            strErrorMsg = "";
            if (var.Length == 0)
            {
                strErrorMsg = "Error input variable";
                return false;
            }
            if (m_watch.m_dte.Debugger.CurrentMode != EnvDTE.dbgDebugMode.dbgBreakMode)
            {
                strErrorMsg = "Only work on debug break mode";
                return false;
            }

            try
            {
                Debugger debugger = m_watch.m_dte.Debugger;
                Expression exp = debugger.GetExpression(var, true, s_nTimeOutMiliSecond);
                if (!exp.IsValidValue)
                {
                    strErrorMsg = exp.Value;
                    return false;
                }

                string strOldValue = exp.Value.ToString();
                string strOldType = exp.Type.ToString();
                if (strOldType.IndexOf("ComPtr") != -1)
                {
                    exp = debugger.GetExpression(exp.Name + ".p", false, s_nTimeOutMiliSecond);
                    if (!exp.IsValidValue)
                    {
                        strErrorMsg = exp.Value;
                        return false;
                    }
                }
                else if (strOldType.IndexOf("auto_ptr") != -1 || strOldType.IndexOf("shared_ptr") != -1)
                {
                    exp = debugger.GetExpression(exp.Name + "._Myptr", false, s_nTimeOutMiliSecond);
                    if (!exp.IsValidValue)
                    {
                        strErrorMsg = exp.Value;
                        return false;
                    }
                }
                else if (strOldType.IndexOf("RefPtr") != -1)
                {
                    exp = debugger.GetExpression(exp.Name + "._ptr", false, s_nTimeOutMiliSecond);
                    if (!exp.IsValidValue)
                    {
                        strErrorMsg = exp.Value;
                        return false;
                    }
                }
                strOldType = exp.Type.ToString();
                if (strOldType.IndexOf("*") == -1)
                {
                    exp = debugger.GetExpression("&(" + exp.Name + ")", false, s_nTimeOutMiliSecond);
                    if (!exp.IsValidValue)
                    {
                        strErrorMsg = exp.Value;
                        return false;
                    }
                }
                string strValue = exp.Value.ToString();
                string strType = exp.Type.ToString();
                m_watch.OutputStr("Var: " + var);
                m_watch.OutputStr("Value: " + strOldValue);
                //m_watch.OutputStr("Address: " + strValue);
                m_watch.OutputStr("Type: " + strType);
                m_watch.OutputStr("Custom watch Info below: \n-------------------------------------");

                int nPos = strValue.IndexOf('{');
                if (nPos > 0)
                {
                    strValue = strValue.Substring(0, nPos).Trim();
                }
                nPos = strType.IndexOf('{');
                if (nPos > 0)
                {
                    strType = strType.Substring(0, nPos).Trim();
                }
                if (strValue == "0xcccccccc" || strValue == "0x00000000" || strValue == "0xcccccccccccccccc" || strValue.Length == 0)
                {
                    strErrorMsg = "Invalid variable mem address：" + exp.Value;
                    return false;
                }


                return CustomWatch(strValue, strType, out strErrorMsg, out strRetValue);
            }
            catch (System.Exception e)
            {
                strErrorMsg = "Watch failed: " + e.ToString();
                return false;
            }
        }

        private bool CustomWatch(string address, string type, out string strErrorMsg, out string strRetValue)
        {
            strErrorMsg = "";
            strRetValue = "";
            string dllName = "";
            string funcName = "";
            WatchConfig.GetAppConfig(out dllName, out funcName);
            if (dllName == "" || funcName == "")
            {
                m_watch.OutputStr("Callback Dll: <" + dllName + ">");
                m_watch.OutputStr("Callback Function: <" + funcName + ">");
                strErrorMsg = "Config Error, please using Alt+3 change the config!";
                return false;
            }
            return CalcCustomWatch(dllName, funcName, address, type, out strErrorMsg, out strRetValue);
        }

        private bool CalcCustomWatch(string dllName, string funcName, string address, string type, out string strErrorMsg, out string strRetValue)
        {
            strErrorMsg = "";
            strRetValue = "";

            string strNewExpr = string.Format("{{,,{0}}}{1}({2}, \"{3}\")", dllName, funcName, address, type);

            try
            {
                Debugger debugger = m_watch.m_dte.Debugger;
                Expression exp = debugger.GetExpression(strNewExpr, true, s_nTimeOutMiliSecond);
                if (!exp.IsValidValue)
                {
                    strErrorMsg = exp.Value;
                    return false;
                }

                string strRetAddress = exp.Value;
                int nIndex = strRetAddress.IndexOf("\"");
                if (nIndex != -1)
                {
                    strRetAddress = strRetAddress.Remove(nIndex).Trim();
                }

                Int64 nAddress = Convert.ToInt64(strRetAddress, 16);
                if (!ReadProcessMemory((ulong)nAddress, out strRetValue, out strErrorMsg))
                {
                    return false;
                }

                if (strRetValue.Length == 0)
                {
                    strErrorMsg = "Read value from mem failed!";
                    return false;
                }
            }
            catch (System.Exception e)
            {
                strErrorMsg = "Query failed: " + e.ToString();
                return false;
            }
            return true;
        }

        private bool ReadProcessMemory(ulong nMemoryAddr, out string sReadContent, out string sError)
        {
            sReadContent = "";
            sError = "";

            try
            {
                DkmStackFrame frame = DkmStackFrame.ExtractFromDTEObject(m_watch.m_dte.Debugger.CurrentStackFrame);
                sReadContent = System.Text.Encoding.ASCII.GetString(
                    frame.Process.ReadMemoryString(nMemoryAddr, DkmReadMemoryFlags.None, 1, s_nMaxReadBufLength));
            }
            catch(System.Exception ex)
            {
                sError = ex.ToString();
                return false;
            }
            return true;
        }

    }
}
