using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;
using System.Security;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Principal;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Web.Services.Description;
using System.Runtime.InteropServices;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.Util;
using Framework.Controller;
using Framework.View;
using SparxEA.Model;
using Framework.Util.SchemaManagement.JSON;
using Framework.ConfigurationManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema.Infrastructure.Collections;
using LibGit2Sharp;

using Plugin.Application.Events.API;
using Plugin.Application.CapabilityModel;
using Plugin.Application.Forms;
using APIManager.SparxEA.Properties;        // Addresses the "settings" environment so we can retrieve run-time settings.
using Atlassian.Jira;

namespace Plugin.Application.Events.Util
{
    class DebugEvent : EventImplementation
    {
        private const string _ServiceDeclPkgStereotype = "ServiceDeclPkgStereotype";

        private JSchema _commonSchema;

        internal override bool IsValidState() { return true; }

        /// <summary>
        /// This event is used for try-outs and stuff.
        /// </summary>
        internal override void HandleEvent()
        {
            try
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                ModelSlt model = ModelSlt.GetModelSlt();

                MEClass currentClass = context.CurrentClass;
                MEPackage currentPackage = context.CurrentPackage;
                Diagram currentDiagram = context.CurrentDiagram;

                string UserName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                String UserName2 = System.DirectoryServices.AccountManagement.UserPrincipal.Current.DisplayName; 
                String UserName3 = Environment.UserName; 
                string UserName5 = System.Windows.Forms.SystemInformation.UserName;

                IntPtr accountToken = WindowsIdentity.GetCurrent().Token;
                WindowsIdentity windowsIdentity = new WindowsIdentity(accountToken);
                string UserName6 = windowsIdentity.Name;

                MessageBox.Show("Usernames: " + UserName + ", " + UserName2 + ", " + UserName3 + ", " + UserName5 + ", " + UserName6);

                //var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
                //CapabilityModel.Service myService = svcContext.GetServiceInstance(););

                //MEPackage findPkg = model.FindPackage("ECDMRoot:DomainModels", "ReleaseHistory");
                //MessageBox.Show("Found my package with ID = " + (findPkg != null ? findPkg.GlobalID : "_NONE_"));

                //MakeCommonSchema();
                //Test7() ;
                //TestSwagger();
                //MessageBox.Show("All Done!");
            }
            catch (Exception exc)
            {
                Logger.WriteError("Oops, Caught exception:" + Environment.NewLine + exc.ToString());
            }
        }


        private void LeaveDotsAndSlashesEscaped()
        {
            var getSyntaxMethod =
                typeof(UriParser).GetMethod("GetSyntax", BindingFlags.Static | BindingFlags.NonPublic);
            if (getSyntaxMethod == null)
            {
                throw new MissingMethodException("UriParser", "GetSyntax");
            }

            object uriParser = getSyntaxMethod.Invoke(null, new object[] { "http" });

            var setUpdatableFlagsMethod =
                uriParser.GetType().GetMethod("SetUpdatableFlags", BindingFlags.Instance | BindingFlags.NonPublic);
            if (setUpdatableFlagsMethod == null)
            {
                throw new MissingMethodException("UriParser", "SetUpdatableFlags");
            }

            setUpdatableFlagsMethod.Invoke(uriParser, new object[] { 0 });
        }

        /// <summary>
        /// A simple input box to request input from user...
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private DialogResult ShowInputDialog(ref string input)
        {
            System.Drawing.Size size = new System.Drawing.Size(200, 70);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = "Name";

            System.Windows.Forms.TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 5);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 39);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 39);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            input = textBox.Text;
            return result;
        }

        void TestSwagger()
        {
            using (StringWriter writer = new StringWriter())
            using (JsonTextWriter wr = new JsonTextWriter(writer))
            {
                wr.Formatting = Newtonsoft.Json.Formatting.Indented;
                wr.WriteStartObject();
                    wr.WritePropertyName("swagger");
                    wr.WriteValue("2.0");
                    wr.WritePropertyName("info");
                    wr.WriteStartObject();
                        wr.WritePropertyName("description");
                        wr.WriteValue("This is a sample Swagger definition file for ECDM.");
                        wr.WritePropertyName("version");
                        wr.WriteValue("1.0.0");
                        wr.WritePropertyName("title");
                        wr.WriteValue("My First Swagger");
                        wr.WritePropertyName("termsOfService");
                        wr.WriteValue("http://swagger.io/terms/");
                        wr.WritePropertyName("contact");
                        wr.WriteStartObject();
                            wr.WritePropertyName("email");
                            wr.WriteValue("apiteam@swagger.io");
                        wr.WriteEndObject();
                        wr.WritePropertyName("license");
                        wr.WriteStartObject();
                            wr.WritePropertyName("name");
                            wr.WriteValue("Apache 2.0");
                            wr.WritePropertyName("url");
                            wr.WriteValue("http://www.apache.org/licenses/LICENSE-2.0.html");
                        wr.WriteEndObject();
                    wr.WriteEndObject();
                    wr.WritePropertyName("host");
                    wr.WriteValue("api.enexis.nl");
                    wr.WritePropertyName("basePath");
                    wr.WriteValue("/v2");
                    wr.WritePropertyName("tags");
                    wr.WriteStartArray();
                        wr.WriteStartObject();
                            wr.WritePropertyName("name");
                            wr.WriteValue("pet");
                            wr.WritePropertyName("description");
                            wr.WriteValue("Everything about your favourite Pets.");
                            wr.WritePropertyName("externalDocs");
                            wr.WriteStartObject();
                                wr.WritePropertyName("description");
                                wr.WriteValue("Find out more...");
                                wr.WritePropertyName("url");
                                wr.WriteValue("http://swagger.io");
                            wr.WriteEndObject();
                        wr.WriteEndObject();
                        wr.WriteStartObject();
                            wr.WritePropertyName("name");
                            wr.WriteValue("store");
                            wr.WritePropertyName("description");
                            wr.WriteValue("Access to Petstore orders.");
                        wr.WriteEndObject();
                        wr.WriteStartObject();
                            wr.WritePropertyName("name");
                            wr.WriteValue("user");
                            wr.WritePropertyName("description");
                            wr.WriteValue("Operations regarding users.");
                            wr.WritePropertyName("externalDocs");
                            wr.WriteStartObject();
                                wr.WritePropertyName("description");
                                wr.WriteValue("Find out more about our store...");
                                wr.WritePropertyName("url");
                                wr.WriteValue("http://swagger.io");
                            wr.WriteEndObject();
                        wr.WriteEndObject();
                    wr.WriteEndArray();
                wr.WriteEndObject();
                wr.Flush();

                string json = writer.ToString();

                using (FileStream saveStream = new FileStream("c://Temp/JSON/TestSwagger.json", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (StreamWriter w = new StreamWriter(saveStream, Encoding.UTF8))
                    {
                        w.Write(json);
                    }
                }
            }


        }


        /// <summary>
        /// This method creates a common-schema JSON file containing an address definitions....
        /// </summary>
        internal void MakeCommonSchema()
        {
            ContextSlt context = ContextSlt.GetContextSlt();

            //Small 'common' schema:
            this._commonSchema = new JSchema
            {
                Id = new Uri("#commonSchema", UriKind.RelativeOrAbsolute),
                SchemaVersion = new Uri(context.GetConfigProperty("JSONSchemaStdNamespace")),
                Title = "root",
                Description = "Nice Common Schema with a load of definitions."
            };

            JSchema AddressType = new JSchema
            {
                Type = JSchemaType.Object,
                Properties =
                {
                    {"StreetName", new JSchema{Type = JSchemaType.String} },
                    {"HouseNumber", new JSchema{Type = JSchemaType.Integer } },
                    {"HouseNumberSuffix", new JSchema{Type = JSchemaType.String} },
                    {"PostalCode", new JSchema{Type = JSchemaType.String} },
                    {"City", new JSchema{Type = JSchemaType.String} }
                }
            };

            JObject myObject = new JObject();
            myObject.Add("AddressType", AddressType);
            this._commonSchema.ExtensionData.Add("Definitions", myObject);


            using (FileStream saveStream = new FileStream("c://Temp/JSON/Common.json", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter writer = new StreamWriter(saveStream, Encoding.UTF8))
                {
                    writer.Write(this._commonSchema.ToString());
                }
            }
        }

        /// <summary>
        /// TEST1 ***************************************
        /// </summary>
        internal void Test1()
        {
            /***************************************************
            ContextSlt context = ContextSlt.GetContextSlt();

            JSchema referencedSchema = new JSchema
            {
                Type = JSchemaType.Object,
                Properties =
                {
                    {"aap", new JSchema{Type = JSchemaType.String} },
                    {"noot", new JSchema{Type = JSchemaType.String} },
                    {"mies", new JSchema{Type = JSchemaType.String} }
                }
            };

            JSchema alsoReferenced = new JSchema
            {
                Type = JSchemaType.Object,
                Properties =
                {
                    {"een", new JSchema{Type = JSchemaType.Integer} },
                    {"twee", new JSchema{Type = JSchemaType.Integer } },
                    {"drie", new JSchema{Type = JSchemaType.Integer} }
                }
            };

            JSchema streetType = new JSchema
            {
                Type = JSchemaType.String,
                MinimumLength = 1,
                MaximumLength = 36
            };

            JSchema myOwnSchema = new JSchema
            {
                Type = JSchemaType.Object,
                Id = new Uri("urn:x-enexis:ecdm:json:myownschema.json"),
                SchemaVersion = new Uri(context.GetConfigProperty("JSONSchemaStdNamespace")),
                Title = "My First JSON (Schema)",
                Description = Environment.NewLine + "Whole lot of schema going on in here. Maybe even a header and a lot of tracing stuff..." +
                              Environment.NewLine + "Spread over multiple lines!",
                Properties =
                {
                    {"FromAddress", this._address },
                    {"ToAddress", this._address }
                }
            };

            JObject myObject = new JObject();
            myObject.Add("AddressType", this._address);
            //myObject.Add("OtherReferencedType", alsoReferenced);
            myOwnSchema.ExtensionData.Add("Definitions", myObject);

            JSchemaDictionary schemaDict = myOwnSchema.Properties as JSchemaDictionary;
            schemaDict.AddItem("StreetsWithoutName",
                                new JSchema
                                {
                                    Type = JSchemaType.Array,
                                    MinimumItems = 0,
                                    MaximumItems = 10,
                                    UniqueItems = true,
                                    Items = { streetType }
                                });

            schemaDict.AddItem("HouseNumber",
                               new JSchema
                               {
                                   Type = JSchemaType.Integer,
                                   Minimum = 1,
                                   Maximum = 999
                               });


            schemaDict.AddItem("ZIPCode", new JSchema { Type = JSchemaType.String });
            schemaDict.AddItem("City", new JSchema { Type = JSchemaType.String });
            schemaDict.AddItem("Country", new JSchema { Type = JSchemaType.String });

            JSchema schema1 = new JSchema() { Type = JSchemaType.String };
            JSchema schema2 = new JSchema() { Type = JSchemaType.Number };
            JSchema schema3 = new JSchema()
            {
                OneOf = { schema1, schema2 }
            };
            schemaDict.AddItem("MyChoice", schema3);
            schemaDict.AddItem("Teller", alsoReferenced);

            using (FileStream saveStream = new FileStream("c://Temp/JSON/mySchema.json", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter writer = new StreamWriter(saveStream, Encoding.UTF8))
                {
                    writer.Write(myOwnSchema.ToString());
                }
            }

            JSONSchema mySchema = new JSONSchema("token", "file:c://Temp/JSON/myOtherSchema.json");
            JSchema inner = mySchema.JSchema;
            ******************************************/
        }

        /// <summary>
        /// TEST2 ***************************************
        /// </summary>
        internal void Test2()
        {
            JSchema schema1 = new JSchema
            {
                Id = new Uri("#foo", UriKind.RelativeOrAbsolute),
                Title = "schema1"
            };

            JSchema schema2 = new JSchema
            {
                Id = new Uri("otherschema.json", UriKind.RelativeOrAbsolute),
                Title = "schema2",
                ExtensionData =
                {
                    {
                        "nested",
                        new JSchema
                        {
                            Title = "nested",
                            Id = new Uri("#bar", UriKind.RelativeOrAbsolute)
                        }
                    },
                    {
                        "alsonested",
                        new JSchema
                        {
                            Title = "alsonested",
                            Id = new Uri("t/inner.json#a", UriKind.RelativeOrAbsolute),
                            ExtensionData =
                            {
                                {
                                    "nestedmore",
                                    new JSchema { Title = "nestedmore" }
                                }
                            }
                        }
                    }
                }
            };
            JSchema schema3 = new JSchema
            {
                Title = "schema3",
                Id = new Uri("some://where.else/completely#", UriKind.RelativeOrAbsolute)
            };

            JSchema root = new JSchema
            {
                Id = new Uri("http://x.y.z/rootschema.json#", UriKind.RelativeOrAbsolute),
                ExtensionData =
                {
                    { "schema1", schema1 },
                    { "schema2", schema2 },
                    { "schema3", schema3 }
                }
            };

            using (FileStream saveStream = new FileStream("c://Temp/JSON/Test1.json", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter writer = new StreamWriter(saveStream, Encoding.UTF8))
                {
                    string rootTxt = root.ToString();
                    writer.Write(rootTxt);
                }
            }
        }


        /// <summary>
        /// TEST3 ***************************************
        /// </summary>
        internal void Test3()
        {
            using (StreamReader file = File.OpenText(@"c:\Temp\JSON\TestSchema.json"))
            using (JsonTextReader reader = new JsonTextReader(file))
            {

                JSchemaUrlResolver resolver = new JSchemaUrlResolver();
                JSchema schema = JSchema.Load(reader, new JSchemaReaderSettings
                {
                    Resolver = resolver,
                    BaseUri = new Uri(@"c:\Temp\JSON\TestSchema.json")
                });

                using (FileStream saveStream = new FileStream("c://Temp/JSON/Test3.json", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (StreamWriter writer = new StreamWriter(saveStream, Encoding.UTF8))
                    {
                        string rootTxt = schema.ToString();
                        writer.Write(rootTxt);
                    }
                }
            }
        }

        /// <summary>
        /// TEST4 ***************************************
        /// </summary>
        internal void Test4()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            JSchema myOwnSchema = new JSchema
            {
                Type = JSchemaType.Object,
                Id = new Uri("urn:x-enexis:ecdm:json:#test4-02"),
                SchemaVersion = new Uri(context.GetConfigProperty("JSONSchemaStdNamespace")),
                Title = "My First JSON (Schema)",
                Description = "Description of Test4"
            };

            myOwnSchema.Properties.Add("BillingAddress", GetSubSchema(@"#/Definitions/AddressType"));
            myOwnSchema.Properties.Add("ShippingAddress", GetSubSchema(@"#/Definitions/AddressType"));

            using (FileStream saveStream = new FileStream("c://Temp/JSON/Test4.json", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter writer = new StreamWriter(saveStream, Encoding.UTF8))
                {
                    string rootTxt = myOwnSchema.ToString();
                    writer.Write(rootTxt);
                }
            }
        }

        /// <summary>
        /// TEST5 ***************************************
        /// </summary>
        internal void Test5()
        {
            ContextSlt context = ContextSlt.GetContextSlt();

            // Create the operation schema...
            JSchema operationSchema = new JSchema
            {
                Type = JSchemaType.Object,
                Id = new Uri("urn:x-enexis:ecdm:json:#operation-name"),
                SchemaVersion = new Uri(context.GetConfigProperty("JSONSchemaStdNamespace")),
                Title = "Operation",
                Description = "Description of Test4",
            };

            // Add common definitions...
            JSchema AddressType = new JSchema
            {
                Type = JSchemaType.Object,
                Properties =
                {
                    {"StreetName", new JSchema{Type = JSchemaType.String} },
                    {"HouseNumber", new JSchema{Type = JSchemaType.Integer } },
                    {"HouseNumberSuffix", new JSchema{Type = JSchemaType.String} },
                    {"PostalCode", new JSchema{Type = JSchemaType.String} },
                    {"City", new JSchema{Type = JSchemaType.String} }
                }
            };

            JSchema PersonType = new JSchema
            {
                Type = JSchemaType.Object,
                Properties =
                {
                    {"FirstName", new JSchema{Type = JSchemaType.String} },
                    {"LastName", new JSchema{Type = JSchemaType.String } },
                    {"BirthDay", new JSchema{Type = JSchemaType.String, Format = "date"} },
                    {"PhoneNumber", new JSchema{Type = JSchemaType.String} },
                    {"Address", AddressType }
                }
            };
            JObject myObject = new JObject();
            myObject.Add("AddressType", AddressType);
            myObject.Add("PersonType", PersonType);
            operationSchema.ExtensionData.Add("definitions", myObject);

            JSchema partyType = new JSchema
            {
                Type = JSchemaType.Object,
                Properties =
                {
                    {"Name", new JSchema{Type = JSchemaType.String} },
                    {"ContactPerson", PersonType },
                    {"Address", AddressType }
                }
            };
            
            JSchema orderType = new JSchema
            {
                Type = JSchemaType.Object,
                Title = "OrderType",
                Description = "Description of OrderType",
            };
            orderType.Properties.Add("Customer", partyType);
            orderType.Properties.Add("SupplierParty", partyType);
            orderType.Properties.Add("BillingAddress", AddressType);
            orderType.Properties.Add("ShippingAddress", AddressType);
            orderType.Properties.Add("OrderDate", new JSchema { Type = JSchemaType.String, Format = "date" });
            orderType.Properties.Add("OrderID", new JSchema { Type = JSchemaType.String });
            orderType.Required.Add("OrderID");
            orderType.Required.Add("OrderDate");

            operationSchema.Properties.Add("OrderType", orderType);
            operationSchema.Required.Add("OrderType"); 

            using (FileStream saveStream = new FileStream("c://Temp/JSON/Test5.json", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter writer = new StreamWriter(saveStream, Encoding.UTF8))
                {
                    string rootTxt = operationSchema.ToString();
                    writer.Write(rootTxt);
                }
            }
        }


        /// <summary>
        /// TEST6 ***************************************
        /// </summary>
        internal void Test6()
        {
            ContextSlt context = ContextSlt.GetContextSlt();

            JSchema nestedRef = new JSchema() { Type = JSchemaType.Boolean };
            JSchema referenceSchema = new JSchema()
            {
                Id = new Uri("http://ecdm.enexis.nl/schemas/json/myoperation/common"),
                Items = { nestedRef }
            };

            JSchema root = new JSchema
            {
                Id = new Uri("#root", UriKind.RelativeOrAbsolute),
                Not = nestedRef
            };

            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
            root.WriteTo(jsonWriter, new JSchemaWriterSettings
            {
                ExternalSchemas = { new ExternalSchema(referenceSchema) },
                Version = SchemaVersion.Draft4
            });

            string json = writer.ToString();

            using (FileStream saveStream = new FileStream("c://Temp/JSON/Test6.json", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter w = new StreamWriter(saveStream, Encoding.UTF8))
                {
                    w.Write(json);
                }
            }
        }

        /// <summary>
        /// TEST7 ***************************************
        /// </summary>
        internal void Test7()
        {
            ContextSlt context = ContextSlt.GetContextSlt();

            // Create the Commpn schema...
            JSchema commonSchema = new JSchema
            {
                Id = new Uri("http://ecdm.enexis.nl/schemas/json/myoperation/common/1/standard#"),
                SchemaVersion = new Uri(context.GetConfigProperty("JSONSchemaStdNamespace")),
                Title = "Operation-Common",
                Description = "Description of Common Schema",
            };

            // Add common definitions...
            var AddressType = new JSchema
            {
                Type = JSchemaType.Object,
                Title = "AddressType",
                Properties =
                {
                    {"StreetName", new JSchema{Type = JSchemaType.String} },
                    {"HouseNumber", new JSchema{Type = JSchemaType.Integer } },
                    {"HouseNumberSuffix", new JSchema{Type = JSchemaType.String} },
                    {"PostalCode", new JSchema{Type = JSchemaType.String} },
                    {"City", new JSchema{Type = JSchemaType.String} }
                }
            };

            var PersonType = new JSchema
            {
                Type = JSchemaType.Object,
                Title = "PersonType",
                Properties =
                {
                    {"FirstName", new JSchema{Type = JSchemaType.String} },
                    {"LastName", new JSchema{Type = JSchemaType.String } },
                    {"BirthDay", new JSchema{Type = JSchemaType.String, Format = "date"} },
                    {"PhoneNumber", new JSchema{Type = JSchemaType.String} },
                    {"Address", AddressType }
                }
            };

            var partyType = new JSchema
            {
                Type = JSchemaType.Object,
                Title = "PartyType",
                Properties =
                {
                    {"Name", new JSchema{Type = JSchemaType.String} },
                    {"ContactPerson", PersonType },
                    {"Address", AddressType }
                }
            };

            var IdentifierType = new JSchema
            {
                Type = JSchemaType.String,
                Title = "IdentifierType",
                Description = "These are annotations that go with the classifier"
            };

            var OpenIDType = new JSchema
            {
                Title = "OpenIDType",
                Type = JSchemaType.Object,
                Properties =
                {
                    {"content", new JSchema{Type = JSchemaType.String} },
                    {"@sourceName", new JSchema{Type = JSchemaType.String} },
                    {"@typeCode", new JSchema{Type = JSchemaType.String} }
                },
                MaximumLength = 200
            };

            JObject myObject = new JObject();
            myObject.Add("AddressType", AddressType);
            myObject.Add("PersonType", PersonType);
            myObject.Add("PartyType", partyType);
            myObject.Add("IdentifierType", IdentifierType);
            myObject.Add("OpenIDType", OpenIDType);
            commonSchema.ExtensionData.Add("definitions", myObject);

            using (StringWriter writer = new StringWriter())
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                commonSchema.WriteTo(jsonWriter, new JSchemaWriterSettings
                {
                    Version = SchemaVersion.Draft4
                });
                string json = writer.ToString();

                using (FileStream saveStream = new FileStream("c://Temp/JSON/Test7-Common.json", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (StreamWriter w = new StreamWriter(saveStream, Encoding.UTF8))
                    {
                        w.Write(json);
                    }
                }
            }

            // The Operation Schema.....
            JSchema GetOrder = new JSchema
            {
                Id = new Uri("http://ecdm.enexis.nl/schemas/json/myoperation/1/standard#"),
                SchemaVersion = new Uri(context.GetConfigProperty("JSONSchemaStdNamespace")),
                Title = "GetOrder",
                Description = "Schema that defines the 'GetOrder' Operation",
            };

            JSchema GetOrderRequest = new JSchema
            {
                Type = JSchemaType.Object,
                Title = "GetOrderRequest"
            };

            JSchema GetOrderResponse = new JSchema
            {
                Type = JSchemaType.Object,
                Title = "GetOrderResponse"
            };

            myObject = new JObject();
            myObject.Add("GetOrderRequest", GetOrderRequest);
            myObject.Add("GetOrderResponse", GetOrderResponse);
            GetOrder.ExtensionData.Add("definitions", myObject);
            GetOrder.Required.Add("GetOrderRequest");
            GetOrder.Required.Add("GetOrderResponse");

            JSchema OrderType = new JSchema
            {
                Type = JSchemaType.Object,
                Title = "OrderType"
            };

            // A list of items
            JSchema VendorPartyAttributeType = new JSchema
            {
                Type = JSchemaType.Array,
                Title = "VendorPartyList",
                AllowAdditionalItems = false,
                MinimumItems = 0,
                MaximumItems = 12
            };
            VendorPartyAttributeType.Items.Add(partyType);
            OrderType.Properties.Add("VendorPartyList", VendorPartyAttributeType);

            OrderType.Properties.Add("Customer", partyType);
            OrderType.Properties.Add("SupplierParty", partyType);
            OrderType.Properties.Add("BillingAddress", AddressType);
            OrderType.Properties.Add("ShippingAddress", AddressType);
            OrderType.Properties.Add("OrderDate", new JSchema { Type = JSchemaType.String, Format = "date" });
            OrderType.Required.Add("OrderID");
            OrderType.Required.Add("OrderDate");

            GetOrderRequest.Properties.Add("OrderID", new JSchema { Type = JSchemaType.String });
            GetOrderResponse.Properties.Add("Order", OrderType);

            // Classifier:
            OrderType.Properties.Add("ExternalOrderID", OpenIDType);

            // Default value
            // Attribute that is based on Classifier, but with additional contents...
            var MyIdentifierType = new JSchema
            {
                Type = IdentifierType.Type,
                Title = IdentifierType.Title,
                Description = "Classifier:" + IdentifierType.Description
            };
            MyIdentifierType.Default = new JValue("ThisIsAnotherDefaultValue");
            OrderType.Properties.Add("OrderID", MyIdentifierType);

            // Extension of complex type:
            var MyOpenIDType = new JSchema
            {
                Default = "DefaultVal"
            };
            MyOpenIDType.AllOf.Add(OpenIDType);
            OrderType.Properties.Add("MyOpenID", MyOpenIDType);

            // Enumeration
            var TypeOrderCode = new JSchema
            {
                Type = JSchemaType.String
            };
            TypeOrderCode.Enum.Add(new JValue("SalesOrder"));
            TypeOrderCode.Enum.Add(new JValue("PurchaseOrder"));
            TypeOrderCode.Enum.Add(new JValue("Quotation"));
            OrderType.Properties.Add("TypeOfOrderCode", TypeOrderCode);


            // Choice
            var ChoiceAddress = new JSchema
            {
                Title = "ChoiceAddress",
                Type = JSchemaType.Object
            };
            ChoiceAddress.OneOf.Add(new JSchema
            {
                Title = "FirstLeg",
                Type = JSchemaType.Object,
                Properties =
                {
                    {"StreetName", new JSchema{Type = JSchemaType.String} },
                    {"HouseNumber", new JSchema{Type = JSchemaType.Integer} },
                    {"HouseNumberSuffix", new JSchema{Type = JSchemaType.String} }
                }
            });
            ChoiceAddress.OneOf.Add(new JSchema
            {
                Title = "SecondLeg",
                Type = JSchemaType.Object,
                Properties =
                {
                    {"POBox", new JSchema{Type = JSchemaType.String} }
                }
            });
            ChoiceAddress.Properties.Add("PostalCode", new JSchema { Type = JSchemaType.String });
            ChoiceAddress.Properties.Add("CityName", new JSchema { Type = JSchemaType.String });
            OrderType.Properties.Add("ChoiceAddress", ChoiceAddress);

            OrderType.Properties.Add("MyAnyType", new JSchema());

            using (StringWriter writer = new StringWriter())
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                GetOrder.WriteTo(jsonWriter, new JSchemaWriterSettings
                {
                    ExternalSchemas = { new ExternalSchema(commonSchema) },
                    Version = SchemaVersion.Draft4
                });
                string json = writer.ToString();

                using (FileStream saveStream = new FileStream("c://Temp/JSON/Test7.json", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (StreamWriter w = new StreamWriter(saveStream, Encoding.UTF8))
                    {
                        w.Write(json);
                    }
                }
            }
        }

        /// <summary>
        /// Function that, given the location and reference to data in a common schema, returns the referenced sub-schema.
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="referenceName"></param>
        /// <returns></returns>
        internal JSchema GetSubSchema(string referenceName)
        {
            SchemaReference schemaRef = new SchemaReference()
            {
                BaseUri = new Uri(string.Empty, UriKind.RelativeOrAbsolute),
                SubschemaId = new Uri(referenceName, UriKind.Relative)
            };
            JSchemaUrlResolver resolver = new JSchemaUrlResolver();
            return resolver.GetSubschema(schemaRef, this._commonSchema);
        }
    }
}
