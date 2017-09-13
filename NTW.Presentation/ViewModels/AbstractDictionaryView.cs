using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Collections;

namespace NTW.Presentation
{
    internal class AbstractDictionaryView : DependencyObject, INotifyPropertyChanged, IDictionary
    {
        public IDictionary Items
        {
            get { return (IDictionary)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(object), typeof(AbstractDictionaryView), new UIPropertyMetadata(null));


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void Change(string Property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(Property));
        }

        #endregion

        #region IDictionary Members

        public void Add(object key, object value)
        {
            Items.Add(key, value);
        }

        public void Clear()
        {
            Items.Clear();
        }

        public bool Contains(object key)
        {
            return Items.Contains(key);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public bool IsFixedSize
        {
            get { return Items.IsFixedSize; }
        }

        public bool IsReadOnly
        {
            get { return Items.IsReadOnly; }
        }

        public ICollection Keys
        {
            get { return Items.Keys; }
        }

        public void Remove(object key)
        {
            Items.Remove(key);
        }

        public ICollection Values
        {
            get { return Items.Values; }
        }

        public object this[object key]
        {
            get
            {
                return Items[key];
            }
            set
            {
                Items[key] = value;
            }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            Items.CopyTo(array, index);
        }

        public int Count
        {
            get { return Items.Count; }
        }

        public bool IsSynchronized
        {
            get { return Items.IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return Items.SyncRoot; }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        #endregion
    }
}
