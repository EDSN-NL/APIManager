using System.IO;
using Newtonsoft.Json;
using Framework.Logging;
using Framework.Context;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Security descriptor singleton provides security settings for the current API generation session.
    /// 
    /// NOTE: Since the chosen API Gateway, WSO2 API Manager, implements its own security model 'outside' the OpenAPI specification,
    /// implementation of this security profile is postponed. ECDM generated OpenAPI 2.0 specifications therefor will NOT contain
    /// any security tags!
    /// </summary>
    internal sealed class RESTSecuritySlt
    {
        // Configuration properties used by this module:
        private const string _OAuthURL          = "OAuthURL";
        private const string _OAuthTokenURL     = "OAuthTokenURL";
        private const string _OAuthRefreshURL   = "OAuthRefreshURL";

        // This is the actual security descriptor singleton. It is created automatically on first load.
        private static readonly RESTSecuritySlt _securityDescriptor = new RESTSecuritySlt();

        // Security schemes supported by the API...
        internal enum Schema { Unknown, OAuth2, APIKey, Basic, None }

        // Different OAuth2 flows supported by the API... 
        private enum OAuth2Flow { Unknown, AuthorizationCode, ClientCredentials, Implicit, Password }

        private Schema _schema;         // Currently selected security schema.

        /// <summary>
        /// Public Security descriptor "factory" method. Simply returns the static instance.
        /// </summary>
        /// <returns>ClassCache singleton object</returns>
        internal static RESTSecuritySlt GetRESTSecuritySlt() { return _securityDescriptor; }

        /// <summary>
        /// (re-)Load security settings from configuration.
        /// </summary>
        internal void Reload()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTSecurity.Reload >> Initializing security descriptor...");
            ContextSlt context = ContextSlt.GetContextSlt();

            // We use a 'brute force' mechanism for translating the various configuration strings to an associated
            // enumeration. This decouples the configuration strings from the internal workings of this descriptor.
            string schemaLabel = context.GetStringSetting(FrameworkSettings._RESTAuthScheme);
            switch (schemaLabel)
            {
                case FrameworkSettings._RESTAuthSchemeAPIKey:
                    this._schema = Schema.APIKey;
                    break;

                case FrameworkSettings._RESTAuthSchemeBasic:
                    this._schema = Schema.Basic;
                    break;

                case FrameworkSettings._RESTAuthSchemeNone:
                    this._schema = Schema.None;
                    break;

                case FrameworkSettings._RESTAuthSchemeOAuth2:
                    this._schema = Schema.OAuth2;
                    break;

                default:
                    this._schema = Schema.Unknown;
                    break;
            }
        }

        /// <summary>
        /// Returns a security scheme object as a JSON-formatted text string. If our scheme is unknown or 'none', an empty string
        /// is returned. Otherwise, we return a schema object according to OpenAPI 2.0 (Swagger) specifications. Since it is formatted
        /// as a JSON object, the scheme is enclosed in braces { scheme-definition }.
        /// </summary>
        /// <returns>Security descriptor, formatted as a JSON String or empty string in case of no- or unknown security.</returns>
        internal string GetSecurityDescriptorSchema()
        {
            switch (this._schema)
            {
                case Schema.OAuth2:
                    return GetOAuth2DescriptorSchema();

                case Schema.APIKey:
                    return GetAPIKeyDescriptorSchema();

                case Schema.Basic:
                    return GetBasicDescriptorSchema();

                default:
                    return string.Empty;
            }
        }

        private string GetAPIKeyDescriptorSchema()
        {
            return string.Empty;
        }

        private string GetBasicDescriptorSchema()
        {
            return string.Empty;
        }

        private string GetOAuth2DescriptorSchema()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            using (StringWriter writer = new StringWriter())
            using (JsonTextWriter json = new JsonTextWriter(writer))
            {
                json.Formatting = Newtonsoft.Json.Formatting.Indented;
                json.WriteStartObject();
                json.WritePropertyName("type"); json.WriteValue("oauth2");

                string flowLabel = context.GetStringSetting(FrameworkSettings._RESTAuthOAuth2Flow);
                switch (flowLabel)
                {
                    case FrameworkSettings._RESTAuthOAuth2FlowAuthCode:
                        json.WritePropertyName("flow"); json.WriteValue("accessCode");
                        break;

                    case FrameworkSettings._RESTAuthOAuth2FlowClientCredentials:

                        break;

                    case FrameworkSettings._RESTAuthOAuth2FlowImplicit:

                        break;

                    case FrameworkSettings._RESTAuthOAuth2FlowPassword:

                        break;

                    default:
                        Logger.WriteWarning(" Configured OAuth2 flow '" + flowLabel + "' is invalid!");
                        return string.Empty;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Private constructor, used to create the static instance.
        /// </summary>
        private RESTSecuritySlt()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTSecurity >> Initializing security descriptor...");
            Reload();
        }
    }
}
