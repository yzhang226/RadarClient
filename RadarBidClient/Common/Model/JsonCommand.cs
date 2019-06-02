using Newtonsoft.Json;
using Radar.Bidding.Model;
using Radar.Common.Enums;
using Radar.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Common.Model
{
    public class JsonCommand
    {

        public long requestId;

        public int clientNo;

        public int directiveVal;

        public string data;

        //
        public int status;

        public string message;

        public JsonCommand()
        {

        }

        public JsonCommand(CommandDirective directive, int status, String data, String message)
        {
            Directive  = directive;
            this.status = status;
            this.data = data;
            this.message = message;
            this.requestId = RequestSequence.me.Next();
        }

        [JsonIgnore]
        public CommandDirective Directive
        {
            get
            {
                return EnumHelper.ToEnum<CommandDirective>(directiveVal);
            }

            set
            {
                this.directiveVal = (int) value;
            }
        }

    }
}
