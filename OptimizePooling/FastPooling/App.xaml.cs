﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows;

namespace FastPooling
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // unique id for global mutex - Global prefix means it is global to the machine
            string mutexId = "Global\\FastPooling";
          
            using (var mutex = new Mutex(false, mutexId))
            {
                // edited by Jeremy Wiebe to add example of setting up security for multi-user usage
                // edited by 'Marc' to work also on localized systems (don't use just "Everyone") 
                var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
                var securitySettings = new MutexSecurity();
                securitySettings.AddAccessRule(allowEveryoneRule);
                mutex.SetAccessControl(securitySettings);

                // edited by acidzombie24
                var hasHandle = false;
                try
                {
                    try
                    {
                        // note, you may want to time out here instead of waiting forever
                        // edited by acidzombie24
                        // mutex.WaitOne(Timeout.Infinite, false);
                        hasHandle = mutex.WaitOne(1000, false);
                        if (!hasHandle)
                        {
                            //throw new TimeoutException("Timeout waiting for exclusive access");
                            Helper.WriteResult(false);
                            MessageBox.Show("Another instance has already been running!");
                        }
                    }
                    catch (AbandonedMutexException)
                    {
                        // Log the fact the mutex was abandoned in another process, it will still get aquired
                        hasHandle = true;
                    }

                    //Perform your work here.
                    if (hasHandle)
                    {
                        try
                        {
                            FastPoolingMainWindow mainWnd = new FastPoolingMainWindow();
                            mainWnd.ShowDialog();
                        }
                        catch (Exception ex)
                        {
                            Helper.WriteResult(false);
                            MessageBox.Show(ex.Message + ex.StackTrace, "Error happened");
                        }
                    }
                }
                finally
                {
                    // edited by acidzombie24, added if statemnet
                    if (hasHandle)
                        mutex.ReleaseMutex();
                    Application.Current.Shutdown();
                }
            }

        }
    }
}
