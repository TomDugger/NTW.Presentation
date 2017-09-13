using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NTW.Commands;

namespace NTW.Presentation
{
    internal class DictionaryViewModel<TKey, TValue>:AbstractDictionaryView
    {
        #region Private
        private TKey selectedKey;
        private TValue defaultValue;

        private Command addCommand;
        private Command removeCommand;
        private Command clearCommand;
        #endregion

        public DictionaryViewModel() { }

        #region Public
        public List<TKey> MKeys
        {
            get 
            {
                List<TKey> temp = new List<TKey>();
                if (Items != null)
                {
                    foreach (TKey item in (Items as IDictionary<TKey, TValue>).Keys)
                        temp.Add(item);
                }
                return temp;
            }
        }

        public TKey SelectedKey
        {
            get { return selectedKey; }
            set { selectedKey = value; Change("Value"); }
        }

        public TValue Value
        {
            get { if(selectedKey != null && Items != null && Items.Contains(selectedKey)) return (Items as IDictionary<TKey, TValue>)[selectedKey]; else return defaultValue; }
            set { (Items as IDictionary<TKey, TValue>)[selectedKey] = value; }
        }
        #endregion
    }
}
