using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections;
using System.ComponentModel;

namespace NTW.Presentation
{
    internal class AbstractView : DependencyObject, INotifyPropertyChanged, IList
    {
        public IList Items
        {
            get { return (IList)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(IList), typeof(AbstractView), new UIPropertyMetadata(null));

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void Change(string Property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(Property));
        }

        #endregion

        #region IList Members

        int IList.Add(object value)
        {
            Items.Add(value);
            Change("AItems");
            return Items.Count;
        }

        public bool Contains(object value)
        {
            return Items.Contains(value);
        }

        public int IndexOf(object value)
        {
            return Items.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            Items.Insert(index, value);
            Change("AItems");
        }

        public bool IsFixedSize
        {
            get { return Items.IsFixedSize; }
        }

        public bool IsReadOnly
        {
            get { return Items.IsReadOnly; }
        }

        public void RemoveAt(int index)
        {
            Items.RemoveAt(index);
            Change("AItems");
        }

        public object this[int index]
        {
            get
            {
                return Items[index];
            }
            set
            {
                Items[index] = value;
            }
        }

        public void Clear()
        {
            Items.Clear();
            Change("AItems");
        }

        public void Remove(object value)
        {
            Items.Remove(value);
            Change("AItems");
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

        public IEnumerator GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        #endregion
    }
}
