using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace NTW.Presentation
{
    internal class KeyValue<TKey, TValue> : INotifyPropertyChanged
    {
        #region Private
        private TKey _firstvalue;
        private TValue _secondValue;
        #endregion

        public KeyValue(TKey key, TValue value) {
            _firstvalue = key;
            _secondValue = value;
        }

        #region Public
        public TKey Key {
            get {
                return _firstvalue;
            }
            set {
                _firstvalue = value;
                Change("FirstValue");
            }
        }

        public TValue Value {
            get { return _secondValue; }
            set {
                _secondValue = value;
                Change("SecondValue");
            }
        } 
        #endregion

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void Change(string Property) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(Property));
        }
        #endregion
    }
}
