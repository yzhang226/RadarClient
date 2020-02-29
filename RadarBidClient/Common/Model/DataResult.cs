using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Common.Model
{
    public class DataResult<T>
    {
        public int Status { get; set; }

        public int HttpStatus { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }

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
