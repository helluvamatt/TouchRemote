using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TouchRemote.Utils
{
    internal class DelegateCommand : ICommand
    {
        private Action _Action;
        private Func<bool> _CanExecCallback;
        private bool prevCanExec = true;

        public DelegateCommand(Action action, Func<bool> canExecCallback = null)
        {
            _Action = action;
            _CanExecCallback = canExecCallback;
        }

        public void Execute(object parameter)
        {
            _Action.Invoke();
        }

        public bool CanExecute(object parameter)
        {
            if (_CanExecCallback != null)
            {
                bool canExec = _CanExecCallback.Invoke();
                if (canExec != prevCanExec) CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                prevCanExec = canExec;
            }
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }

    internal class DelegateCommand<T> : ICommand
    {
        private Action<T> _Action;
        private Func<T, bool> _CanExecCallback;
        private bool prevCanExec = true;

        public DelegateCommand(Action<T> action, Func<T, bool> canExecCallback = null)
        {
            _Action = action;
            _CanExecCallback = canExecCallback;
        }

        public void Execute(object parameter)
        {
            _Action.Invoke((T)parameter);
        }

        public bool CanExecute(object parameter)
        {
            if (_CanExecCallback != null)
            {
                bool canExec = _CanExecCallback.Invoke((T)parameter);
                if (canExec != prevCanExec) CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                prevCanExec = canExec;
            }
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
