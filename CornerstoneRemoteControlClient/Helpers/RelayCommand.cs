// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.Diagnostics;
using System.Windows.Input;

namespace CornerstoneRemoteControlClient.Helpers
{
    /// <summary>
    ///     A command whose sole purpose is to relay its functionality to other
    ///     objects by invoking delegates. The default return value for the CanExecute
    ///     method is 'true'.
    ///     Adapted from the MSDN Magazine article "WPF Apps With The Model-View-ViewModel
    ///     Design Pattern" by Josh Smith.
    /// </summary>
    public class RelayCommand : ICommand
    {
        /// <summary>
        ///     Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <param name="hasPermission">The has permission.</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null, Func<bool> hasPermission = null)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
            _hasPermission = hasPermission;
        }

        #region ICommand Members

        [DebuggerStepThrough]
        public Boolean CanExecute(Object o)
        {
            return (_canExecute == null || _canExecute()) && (_hasPermission == null || _hasPermission());
        }

        /// <summary>
        ///     Occurs when changes occur that affect whether or not the command should
        ///     execute.  Note that this delegates to CommandManager.RequerySuggested, which
        ///     holds a weak reference, so a strong reference to the event handler should
        ///     be held by the calling class.  For efficiency, the event handler is also only
        ///     added if there is a _canExecute function.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null || _hasPermission != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (_canExecute != null || _hasPermission != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public void Execute(Object o)
        {
            _execute();
        }

        #endregion

        #region Implementation of IHasPermission

        public bool HasPermission()
        {
            return (_hasPermission == null || _hasPermission());
        }

        #endregion

        #region Fields

        private readonly Func<bool> _canExecute;
        private readonly Action _execute;
        private readonly Func<bool> _hasPermission;

        #endregion // Fields
    }
}
