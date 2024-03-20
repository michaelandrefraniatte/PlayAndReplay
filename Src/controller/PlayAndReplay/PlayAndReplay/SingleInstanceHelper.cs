using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenWithSingleInstance
{
    static class SingleInstanceHelper
    {
        /// <summary>
        /// Checks if another instance of this app is running by using a named Mutex
        /// </summary>
        /// <returns>True if another istance is already running</returns>
        public static bool CheckInstancesUsingMutex()
        {
            Mutex _appMutex = new Mutex(false, "OpenWithSingleInstance");
            if (!_appMutex.WaitOne(1000))
                return true;//There is another instance

            return false;
        }

        /// <summary>
        /// Checks if another instance of this app is running by counting the number of processes
        /// </summary>
        /// <returns>True if another istance is already running</returns>
        public static bool CheckInstancesFromRunningProcesses()
        {
            //Get current process info
            Process _currentProcess = Process.GetCurrentProcess();
            //Get all the processes with this name
            Process[] _allProcesses = Process.GetProcessesByName(_currentProcess.ProcessName);

            //If there is more than 1 process with this name
            if (_allProcesses.Length > 1)
                return true;//There is another instance

            return false;
        }

        public static Process GetAlreadyRunningInstance()
        {
            Process _currentProc = Process.GetCurrentProcess();
            Process[] _allProcs = Process.GetProcessesByName(_currentProc.ProcessName);

            for (int i = 0; i < _allProcs.Length; i++)
            {
                if (_allProcs[i].Id != _currentProc.Id)
                    return _allProcs[i];
            }

            return null;
        }
    }
}