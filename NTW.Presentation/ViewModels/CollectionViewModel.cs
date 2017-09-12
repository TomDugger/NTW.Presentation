using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using NTW.Commands;
using NTW.Presentation;
using System.Windows.Data;
using System.Collections;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace NTW.Presentation
{
    /// <summary>
    /// Внутренее ViewModel предназначенная для управления списками.
    /// </summary>
    internal class CollectionViewModel<T> : AbstractView
    {

        #region Private
        private Command addCommand;
        private Command removeCommand;
        private Command clearCommand;
        #endregion

        public CollectionViewModel() { }

        #region Public
        public Command AddCommand {
            get {
                return addCommand ?? (addCommand = new Command(obj =>
                {

                    object value;

                    if (typeof(T) == typeof(string))
                        value = Activator.CreateInstance(typeof(T), new object[] { "value".ToCharArray() });
                    else
                        value = Activator.CreateInstance(typeof(T));

                    Items.Add(value);
                    Change("Items");
                    Change("AItems");
                }, obj => Items != null));
            }
        }

        public Command RemoveCommand {
            get { return removeCommand ?? (removeCommand = new Command(obj => {

                Items.Remove(obj);
                Change("Items");
                Change("AItems");
            }, obj => Items != null)); }
        }

        public Command ClearCommand {
            get {
                return clearCommand ?? (clearCommand = new Command(obj => {

                    Items.Clear();
                    Change("Items");
                    Change("AItems");
                }, obj => Items != null));
            }
        }

        public List<SimpleTypeItem<T>> AItems
        {
            get
            {
                List<SimpleTypeItem<T>> temp = new List<SimpleTypeItem<T>>();
                if (Items != null)
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        SimpleTypeItem<T> obj = new SimpleTypeItem<T>(i, Items[i]);
                        obj.PropertyChanged += new PropertyChangedEventHandler(obj_PropertyChanged);
                        temp.Add(obj);
                    }
                    return temp;
                }
                return null;
            }
        }

        void obj_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Items != null)
            {
                Items[(sender as SimpleBaseAbstract).ObjectIndex] = (sender as SimpleBaseAbstract).ObjectValue;
                Change(string.Empty);
            }
        }
        #endregion
    }
}
