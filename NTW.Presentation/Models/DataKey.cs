using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace NTW.Presentation
{
    public class DataKey<TKey>: INotifyPropertyChanged, IDataErrorInfo
    {
        #region Private
        private TKey _firstvalue;
        private List<TKey> Keys;
        #endregion

        public DataKey(TKey key, List<TKey> keys)
        {
            Keys = keys;
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

        #region IDataErrorInfo Members

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get {
                string error = String.Empty;
                switch (columnName)
                {
                    case "Value":
                        {
                            TKey f = _firstvalue;
                            if (Keys != null)
                                if (Keys.Contains(f))
                                    error = "ror";
                            break;
                        }
                }
                return error;
            }
        }

        #endregion
    }
}
