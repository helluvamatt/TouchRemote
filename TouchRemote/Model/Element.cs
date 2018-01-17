using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TouchRemote.Model
{
    public abstract class Element : DependencyObject, INotifyPropertyChanged
    {
        #region Dependency properties

        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(double), typeof(Element));
        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(double), typeof(Element));

        public double X
        {
            get
            {
                return (double)GetValue(XProperty);
            }
            set
            {
                SetValue(XProperty, value);
            }
        }

        public double Y
        {
            get
            {
                return (double)GetValue(YProperty);
            }
            set
            {
                SetValue(YProperty, value);
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public Guid Id { get; set; }

        public abstract Dictionary<string, string> WebProperties { get; }

        public abstract bool CanHandleEvent(string eventName);

        public abstract void ProcessEvent(string eventName, object eventData);

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.Property.Name));
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}]", GetType().Name, Id);
        }
    }
}
