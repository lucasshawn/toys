using Rhyous.SimpleArgs;
using System;
using System.Collections.Generic;

namespace Oracle.Oci
{
    // Add this line of code to Main() in Program.cs
    //
    //   ArgsManager.Instance.Start(new ArgsHandler(), args);
    //

    /// <summary>
    /// A class that implements IArgumentsHandler where command line
    /// arguments are defined.
    /// </summary>
    public sealed class ArgsHandler : ArgsHandlerBase
    {
        public ArgsHandler()
        {
            Arguments = new List<Argument>
            {
                new Argument
                {
                    Name = "tenancyid",
                    ShortName = "t",
                    Description = "The OCID for the tenancy",
                    Example = "ocid1.tenancy.oc1..aaaaaaaaba3pv6wkcr4jqae5f15p2b2m2yt2j6rx32uzr4h25vqstifsfdsq"
                },
                new Argument
                {
                    Name = "compartmentid",
                    ShortName = "c",
                    Description = "The OCID for the compartment",
                    Example = "ocid1.compartment.oc1..aaaaaaaam3we6vgnherjq5q2idnccdflvjsnog7mlr6rtdb25gilchfeyjxa"
                },
                new Argument
                {
                    Name = "userId",
                    ShortName = "u",
                    Description = "The OCID for the user",
                    Example = "ocid1.user.oc1..aaaaaaaat5nvwcna5j6aqzjcaty5eqbb6qt2jvpkanghtgdaqedqw3rynjq"
                },
                new Argument
                {
                    Name = "fingerprint",
                    ShortName = "f",
                    Description = "The fingerprint for the public key",
                    Example = "20:3b:97:13:55:1c:5b:0d:d3:37:d8:50:4e:c5:3a:34"
                },
                new Argument
                {
                    Name = "privateKeyPath",
                    ShortName = "p",
                    Description = "Path to the private key, including the PEM file name",
                    Example = "..\\.ssh\\privatekey.pem"
                },
                new Argument
                {
                    Name = "privatekeypassword",
                    ShortName = "pw",
                    Description = "The password for the key (if one exists)",
                    Example = "xyz123!@#"
                },
            };
        }

        public override void HandleArgs(IReadArgs inArgsHandler)
        {
            base.HandleArgs(inArgsHandler);
         }
    }
}
