using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NTW.Commands;

namespace NTW.Presentation.ViewModels
{
    internal class DictionaryViewModel<TKey, TValue>:AbstractView
    {
        #region Private
        private Command addCommand;
        private Command removeCommand;
        private Command clearCommand;
        #endregion

        public DictionaryViewModel() { }

        #region Public
        public List<SimpleTypeItem<TKey>> Keys
        {
            get 
            {
                List<SimpleTypeItem<TKey>> temp = new List<SimpleTypeItem<TKey>>();
                if (Items != null)
                {
                    int index = 0;
                    foreach(TKey item in (Items as IDictionary<TKey, TValue>).Keys)
                    {
                        SimpleTypeItem<TKey> obj = new SimpleTypeItem<TKey>(index, item);
                        index++;
                    }
                }
                return temp;
            }
        }

        public List<SimpleTypeItem<TValue>> Values
        {
            get
            {
                List<SimpleTypeItem<TValue>> temp = new List<SimpleTypeItem<TValue>>();

                if (Items != null)
                {
                    int index = 0;
                    foreach(TValue item in (Items as IDictionary<TKey, TValue>).Values)
                    {
                        SimpleTypeItem<TValue> obj = new SimpleTypeItem<TValue>(index, item);
                        index++;
                    }
                }

                return temp;
            }
        }
        #endregion
    }
}
