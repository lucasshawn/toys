﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// Version 1.0.1
namespace Oracle.Oci
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using Newtonsoft.Json;

    //
    //  Nuget Package Manager Console: Install-Package BouncyCastle
    //  Nuget CLI: nuget install BouncyCastle
    //
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.OpenSsl;
    using Org.BouncyCastle.Security;
    using Rhyous.SimpleArgs;

    public class Signing
    {
        static private string parseArg(string[] args, string argName, string argNameShortcut, bool isRequired, string defaultStr = "")
        {
            int idx = -1;
            for (idx = 0; idx < args.Count(); idx++)
            {
                if (args[idx].Equals($"--{argName}", StringComparison.InvariantCultureIgnoreCase) || 
                    args[idx].Equals($"-{argNameShortcut}", StringComparison.InvariantCultureIgnoreCase))
                    break;
            }
            if (idx == args.Count())
            {
                if (isRequired)
                {
                    Console.WriteLine($"Missing argument {argName}.");
                    throw new InvalidOperationException();
                }
                return defaultStr;
            }
            else
                return args[++idx];
        }

        static private void Help()
        {
            Console.WriteLine("oci-curl-cs [--tenancyid={tenancyid} --compartmentid={compartmentid} --userid={userid} --fingerprint={fingerprint} --privatekeypath={path to private key file} --password={password}] | [--profile {OCI profile name} --compartmentid={compartmentid}]");
            Console.WriteLine(string.Format("{0,-25} The OCI tenancy id", "--tenancyid | -t"));
            Console.WriteLine(string.Format("{0,-25} The OCI compartment id", "--compartmentid | -c"));
            Console.WriteLine(string.Format("{0,-25} The OCI user id", "--userid | -u"));
            Console.WriteLine(string.Format("{0,-25} The Fingerprint used for private key", "--fingerprint | -f"));
            Console.WriteLine(string.Format("{0,-25} The private key path to the key file", "--privatekeypath | -k"));
            Console.WriteLine(string.Format("{0,-25} The password to the key file(if any) ", "--password | -p"));
            Console.WriteLine(string.Format("{0,-25} The profile for OCI (use as alternative to entering the details above individually)", "--profile"));
        }

        static private void ShowValues(string tenancyId, string userId, string fingerprint, string privateKeyPath, string password)
        {
            Console.WriteLine("Using values to connect to OCI:");
            Console.WriteLine($"\ttenancyId: {tenancyId}");
            Console.WriteLine($"\tuserId: {userId}");
            Console.WriteLine($"\tfingerprint: {fingerprint}");
            Console.WriteLine($"\tprivateKeyPath: {privateKeyPath}");
            Console.WriteLine($"\tpassword: {password}");
        }
        public static int Main(string[] args)
        {
            string  tenancyId = string.Empty, 
                    compartmentId = string.Empty, 
                    userId = string.Empty, 
                    fingerprint = string.Empty, 
                    privateKeyPath = string.Empty, 
                    privateKeyPassphrase = string.Empty, 
                    profile = string.Empty;
            try
            {
                // Optional Profile or the rest.  If Profile, we also need compartment.
                profile = parseArg(args, "profile", string.Empty, false);
                if (string.IsNullOrEmpty(profile))
                {
                    tenancyId = parseArg(args, "tenancyid", "t", true);
                    compartmentId = parseArg(args, "compartmentid", "c", true);
                    userId = parseArg(args, "userid", "u", true);
                    fingerprint = parseArg(args, "fingerprint", "f", true);
                    privateKeyPath = parseArg(args, "privatekeypath", "k", true);
                    privateKeyPassphrase = parseArg(args, "password", "p", true);
                }
                else
                {
                    compartmentId = parseArg(args, "compartmentid", "c", true);
                }
            }
            catch(InvalidOperationException)
            {
                Help();
                return 1;
            }

            if (!string.IsNullOrEmpty(profile))
            {
                var parser = new Ini(Environment.ExpandEnvironmentVariables("%userprofile%//.oci/config"));
                userId = parser.GetValue("user", profile);
                fingerprint = parser.GetValue("fingerprint", profile);
                tenancyId = parser.GetValue("tenancy", profile);
                privateKeyPath = parser.GetValue("key_file", profile);
            }

            ShowValues(tenancyId, userId, fingerprint, privateKeyPath, "####");
            var signer = new RequestSigner(tenancyId, userId, fingerprint, privateKeyPath, privateKeyPassphrase);

            // Oracle Cloud Infrastructure APIs require TLS 1.2
            // uncomment the line below if targeting < .NET Framework 4.6 (HttpWebRequest does not enable TLS 1.2 by default in earlier versions)
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // GET with query parameters (gets user)
            var uri = new Uri($"https://identity.us-phoenix-1.oraclecloud.com/20160918/users/{userId}");
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            request.Accept = "application/json";

            signer.SignRequest(request);

            Console.WriteLine($"Authorization header: {request.Headers["authorization"]}");

            ExecuteRequest(request);

            // POST with body (creates a VCN)
            uri = new Uri($"https://iaas.us-phoenix-1.oraclecloud.com/20160918/vcns");
            var body = string.Format(@"{{""cidrBlock"" : ""10.0.0.0/16"",""compartmentId"" : ""{0}"",""displayName"" : ""MyVcn""}}", compartmentId);
            var bytes = Encoding.UTF8.GetBytes(body);

            request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Headers["x-content-sha256"] = Convert.ToBase64String(SHA256.Create().ComputeHash(bytes));

            using (var stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            signer.SignRequest(request);

            Console.WriteLine($"Authorization header: {request.Headers["authorization"]}");

            ExecuteRequest(request);



            // GET with query parameters (gets namespace)
            uri = new Uri($"https://objectstorage.us-phoenix-1.oraclecloud.com/n/");
            request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            request.Accept = "application/json";

            signer.SignRequest(request);

            Console.WriteLine($"Authorization header: {request.Headers["authorization"]}");

            string namespaceName = ExecuteRequest(request);

            namespaceName = JsonConvert.DeserializeObject<string>(namespaceName);

            // POST with body (creates a bucket)
            uri = new Uri($"https://objectstorage.us-phoenix-1.oraclecloud.com/n/{namespaceName}/b/");
            body = string.Format(@"{{""name"" : ""bucket01"",""compartmentId"" : ""{0}"",""publicAccessType"" : ""ObjectRead""}}", compartmentId);
            bytes = Encoding.UTF8.GetBytes(body);

            request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Headers["x-content-sha256"] = Convert.ToBase64String(SHA256.Create().ComputeHash(bytes));

            using (var stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            signer.SignRequest(request);

            Console.WriteLine($"Authorization header: {request.Headers["authorization"]}");

            ExecuteRequest(request);


            // PUT with body (puts a object)
            uri = new Uri($"https://objectstorage.us-phoenix-1.oraclecloud.com/n/{namespaceName}/b/bucket01/o/object01");
            body = "Hello Object Storage Service!!!";
            bytes = Encoding.UTF8.GetBytes(body);

            request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "PUT";
            request.Accept = "application/json";
            request.ContentType = "application/json";

            using (var stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            signer.SignRequest(request, true);

            Console.WriteLine($"Authorization header: {request.Headers["authorization"]}");

            ExecuteRequest(request);

            // POST with body (create multipart upload)
            uri = new Uri($"https://objectstorage.us-phoenix-1.oraclecloud.com/n/{namespaceName}/b/bucket01/u");
            body = "{\"object\" : \"object02\"}";
            bytes = Encoding.UTF8.GetBytes(body);

            request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Headers["x-content-sha256"] = Convert.ToBase64String(SHA256.Create().ComputeHash(bytes));

            using (var stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            signer.SignRequest(request);

            Console.WriteLine($"Authorization header: {request.Headers["authorization"]}");

            ExecuteRequest(request);

            Console.ReadKey();
            return 0;
        }

        private static string ExecuteRequest(HttpWebRequest request)
        {
            try
            {
                var webResponse = (HttpWebResponse)request.GetResponse();
                var response = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
                Console.WriteLine($"Response: {response}");

                return response;
            }
            catch (WebException e)
            {
                Console.WriteLine($"Exception occurred: {e.Message}");
                Console.WriteLine($"Response: {new StreamReader(e.Response.GetResponseStream()).ReadToEnd()}");

                return String.Empty;
            }
        }

        public class RequestSigner
        {
            private static readonly IDictionary<string, List<string>> RequiredHeaders = new Dictionary<string, List<string>>
            {
                { "GET", new List<string>{"date", "(request-target)", "host" }},
                { "HEAD", new List<string>{"date", "(request-target)", "host" }},
                { "DELETE", new List<string>{"date", "(request-target)", "host" }},
                { "PUT", new List<string>{"date", "(request-target)", "host", "content-length", "content-type", "x-content-sha256" }},
                { "POST", new List<string>{"date", "(request-target)", "host", "content-length", "content-type", "x-content-sha256" }},
                { "PUT-LESS", new List<string>{"date", "(request-target)", "host" }}
            };

            private readonly string keyId;
            private readonly ISigner signer;

            /// <summary>
            /// Adds the necessary authorization header for signed requests to Oracle Cloud Infrastructure services.
            /// Documentation for request signatures can be found here: https://docs.us-phoenix-1.oraclecloud.com/Content/API/Concepts/signingrequests.htm
            /// </summary>
            /// <param name="tenancyId">The tenancy OCID</param>
            /// <param name="userId">The user OCID</param>
            /// <param name="fingerprint">The fingerprint corresponding to the provided key</param>
            /// <param name="privateKeyPath">Path to a PEM file containing a private key</param>
            /// <param name="privateKeyPassphrase">An optional passphrase for the private key</param>
            public RequestSigner(string tenancyId, string userId, string fingerprint, string privateKeyPath, string privateKeyPassphrase = "")
            {
                // This is the keyId for a key uploaded through the console
                this.keyId = $"{tenancyId}/{userId}/{fingerprint}";

                AsymmetricCipherKeyPair keyPair;
                using (var fileStream = File.OpenText(privateKeyPath))
                {
                    try
                    {
                        keyPair = (AsymmetricCipherKeyPair)new PemReader(fileStream, new Password(privateKeyPassphrase.ToCharArray())).ReadObject();
                    }
                    catch (InvalidCipherTextException)
                    {
                        throw new ArgumentException("Incorrect passphrase for private key");
                    }
                }

                RsaKeyParameters privateKeyParams = (RsaKeyParameters)keyPair.Private;
                this.signer = SignerUtilities.GetSigner("SHA-256withRSA");
                this.signer.Init(true, privateKeyParams);
            }

            public void SignRequest(HttpWebRequest request, bool useLessHeadersForPut = false)
            {
                if (request == null) { throw new ArgumentNullException(nameof(request)); }

                // By default, request.Date is DateTime.MinValue, so override to DateTime.UtcNow, but preserve the value if caller has already set the Date
                if (request.Date == DateTime.MinValue) { request.Date = DateTime.UtcNow; }

                var requestMethodUpper = request.Method.ToUpperInvariant();
                var requestMethodKey = useLessHeadersForPut ? requestMethodUpper + "-LESS" : requestMethodUpper;

                List<string> headers;
                if (!RequiredHeaders.TryGetValue(requestMethodKey, out headers))
                {
                    throw new ArgumentException($"Don't know how to sign method: {request.Method}");
                }

                // for PUT and POST, if the body is empty we still must explicitly set content-length = 0 and x-content-sha256
                // the caller may already do this, but we shouldn't require it since we can determine it here
                if (request.ContentLength <= 0 && (string.Equals(requestMethodUpper, "POST") || string.Equals(requestMethodUpper, "PUT")))
                {
                    request.ContentLength = 0;
                    request.Headers["x-content-sha256"] = Convert.ToBase64String(SHA256.Create().ComputeHash(new byte[0]));
                }

                var signingStringBuilder = new StringBuilder();
                var newline = string.Empty;
                foreach (var headerName in headers)
                {
                    string value = null;
                    switch (headerName)
                    {
                        case "(request-target)":
                            value = buildRequestTarget(request);
                            break;
                        case "host":
                            value = request.Host;
                            break;
                        case "content-length":
                            value = request.ContentLength.ToString();
                            break;
                        default:
                            value = request.Headers[headerName];
                            break;
                    }

                    if (value == null) { throw new ArgumentException($"Request did not contain required header: {headerName}"); }
                    signingStringBuilder.Append(newline).Append($"{headerName}: {value}");
                    newline = "\n";
                }

                // generate signature using the private key
                var bytes = Encoding.UTF8.GetBytes(signingStringBuilder.ToString());
                this.signer.BlockUpdate(bytes, 0, bytes.Length);
                var signature = Convert.ToBase64String(this.signer.GenerateSignature());
                var authorization = $@"Signature version=""1"",headers=""{string.Join(" ", headers)}"",keyId=""{keyId}"",algorithm=""rsa-sha256"",signature=""{signature}""";
                request.Headers["authorization"] = authorization;
            }

            private static string buildRequestTarget(HttpWebRequest request)
            {
                // ex. get /20160918/instances
                return $"{request.Method.ToLowerInvariant()} {request.RequestUri.PathAndQuery}";
            }
        }

        /// <summary>
        /// Implements Bouncy Castle's IPasswordFinder interface to allow opening password protected private keys.
        /// </summary>
        public class Password : IPasswordFinder
        {
            private readonly char[] password;

            public Password(char[] password) { this.password = password; }

            public char[] GetPassword() { return (char[])password.Clone(); }
        }
    }
}

