using System;
using System.ComponentModel;
using System.Management.Automation;


namespace adlib {
    [Cmdlet(VerbsCommon.Find, "AD")]
    public class FindAdCommand : Cmdlet {
        string objType;
        string[] inputFilter;
        string[] outputFilter;
        string[] args;

        [Parameter(Position = 0, Mandatory = true)]
        [ValidateSet("User", "Computer", "Group", IgnoreCase = true)]
        public string ObjectType { 
            get { return objType; } 
            set { objType = value; } 
        }

        [Parameter(Mandatory = true)]
        public string[] InputFilter {
            get { return inputFilter; }
            set { inputFilter = value; }
        }

        [Parameter(Mandatory = true)]
        public string[] OutputFilter {
            get { return outputFilter; }
            set { outputFilter = value; }
        }

        [Parameter(Mandatory = true)]
        public string[] Arguments {
            get { return args; }
            set { args = value; }
        }

        protected override void BeginProcessing() {
            if(InputFilter.Length != args.Length)
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("InputFilter length not equal to Argument length"), "ArgumentException", ErrorCategory.InvalidData, args));
            base.BeginProcessing();
        }

        protected override void ProcessRecord() {
            Output output = new Output();
            output.Type = ObjectType;
            output.InputFilter = InputFilter;
            output.OutputFilter = OutputFilter;
            output.Arguments = Arguments;
            WriteObject(output);
        }
    }

    public class Output {
        string type;
        string[] inputFilter;
        string[] outputFilter;
        string[] arguments;
        public Output() { }
        public string Type { get { return type; } set { type = value; } }
        public string[] InputFilter { get { return inputFilter; } set { inputFilter = value; } }
        public string[] OutputFilter { get { return outputFilter; } set { outputFilter = value; } }
        public string[] Arguments { get { return arguments; } set { arguments = value; } }
    }

    [RunInstaller(true)]
    public class ADSnapin : PSSnapIn {
        public ADSnapin() : base() { }

        public override string Name { get { return "ADQuery"; } }
        public override string Vendor { get { return "The364"; } }
        public override string VendorResource { get { return "ADQuery,The364"; } }
        public override string Description { get { return "Cmdlet version of ADQuery for querying Active Directory"; } }
        public override string DescriptionResource { get { return "Cmdlet version of ADQuery for querying Active Directory"; } }
    } 
}