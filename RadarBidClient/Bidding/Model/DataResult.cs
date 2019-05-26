using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Model
{
    public class DataResult<T>
    {
        public int Status;

        public int HttpStatus;

        public string Message;

        public T Data;

        public DataResult()
        {

        }

        public DataResult(int Status, T Data, string Message)
        {
            this.Status = Status;
            this.Data = Data;
            this.Message = Message;
        }

    }
}
