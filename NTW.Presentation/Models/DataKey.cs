using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace NTW.Presentation
{
    public class DataKey<TKey>: INotifyPropertyChanged
    {
        #region Private
        private TKey _firstvalue;
        #endregion

        public DataKey(TKey key)
        {
            _firstvalue = key;
        }

        #region Public
        public TKey Value
        {
            get
            {
                return _firstvalue;
            }
            set
            {
                _firstvalue = value;
                Change("Key");
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
