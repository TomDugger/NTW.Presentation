﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace NTW.Presentation
{
    public class DictionatyNewItem<TKey, TValue>: INotifyPropertyChanged
    {
        #region Private
        private DataKey<TKey> _Key;
        private TValue _Value;
        private bool _IsOpen = false;
        private int _SelectedIndex = 0;
        #endregion

        public DictionatyNewItem() {
            _Key = new DataKey<TKey>((TKey)CreateObject(typeof(TKey)));
            _Value = (TValue)CreateObject(typeof(TValue));
        }

        private object CreateObject(Type type) {
            object value;
            if (type == typeof(string))
                value = Activator.CreateInstance(type, new object[] { "value".ToCharArray() });
            else
                value = Activator.CreateInstance(type);

            return value;
        }

        #region Public
        public DataKey<TKey> Key { get { return _Key; } set { _Key = value; Change("Key"); } }
        public TValue Value { get { return _Value; } set { _Value = value; Change("Value"); } }
        public bool IsOpen { get { return _IsOpen; } set { _IsOpen = value; Change("IsOpen"); } }
        public int SelectedIndex { get { return _SelectedIndex; } set { _SelectedIndex = value; Change("SelectedIndex"); } }
        #endregion

        #region Void
        public void Default()
        {
            Key = new DataKey<TKey>((TKey)CreateObject(typeof(TKey)));
            Value = (TValue)CreateObject(typeof(TValue));
        }
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void Change(string Property) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(Property));
        }

        #endregion
    }
}
