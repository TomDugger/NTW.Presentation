using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NTW.Presentation.Models
{
    public class Result<T>
    {
        public Result(T result)
        {
            ResultChanage = result;

            ActiveResult = true;
        }

        public Result(T altreResult, bool active)
        {
            ResultChanage = altreResult;
            active = false;
        }

        public Result(bool active)
        {
            ActiveResult = active;
        }

        public T ResultChanage { get; set; }

        public bool ActiveResult { get; set; }
    }
}
