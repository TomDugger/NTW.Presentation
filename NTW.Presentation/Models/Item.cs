using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace NTW.Presentation
{
    public class Item<T>:INotifyPropertyChanged
    {
        #region Folders
        private int _index = 0;
        private T _value;
        #endregion

        public Item(int index, T value) {
            _index = index;
            _value = value;
        }

        #region Property
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
        #endregion

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void Change(string Property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(Property));
        }
        #endregion
    }
}
