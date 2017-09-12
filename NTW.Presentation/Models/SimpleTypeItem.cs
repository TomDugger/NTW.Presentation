using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;

namespace NTW.Presentation
{
    internal class SimpleTypeItem<T>:SimpleBaseAbstract
    {
        #region Private
        private int _index = 0;
        private T _value;
        #endregion

        public SimpleTypeItem(int index, object value)
        {
            _index = index;
            _value = (T)value;
        }

        #region Public
        public int Index { get { return _index; } }

        public T Value {
            get { 
                return _value; 
            } 
            set { 
                _value = value;
                Change("Value");
            } 
        }

        public override object ObjectValue {
            get {
                return _value;
            }
            set {
                _value = (T)value;
            }
        }

        public override int ObjectIndex {
            get {
                return _index;
            }
        }
        #endregion
    }

    internal abstract class SimpleBaseAbstract:INotifyPropertyChanged
    {
        public abstract object ObjectValue { get; set; }

        public abstract int ObjectIndex { get; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void Change(string Property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(Property));
        }

        #endregion
    }
}
