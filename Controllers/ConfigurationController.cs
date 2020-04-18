using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;

namespace IGAS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigurationController : ControllerBase
    {
        IConfiguration _config;
        public ConfigurationController(IConfiguration config)
        {
            _config = config;
        }

        /// The code in the controller IS NOT SOMETHING YOU WILL EVER NEED TO WRITE!
        /// This code is for demonstration purposes, you will almost certainly never
        /// have a scenario where you care which provider(s) are used. The convoluted-ness
        /// of the code is an indication of that.
        [HttpGet]
        public IEnumerable<ProviderViewModel> Get()
        {
            // For simplicity and to encourage experimentation, this code will retrieve
            // all configuration settings in the system, with the caveat of environment 
            // variables being pulled in only if prefixed with "IGAS_". This is because of how the
            // environment variable configuration provider is set up in program.cs. The
            // prefix will be stripped automatically, so you will not see "IGAS_" in your setting name.

            var providers = ((IConfigurationRoot)_config).Providers;

            List<ProviderViewModel> providerViewModels = new List<ProviderViewModel>();

            foreach (var provider in providers)
            {
                ProviderViewModel mdl = new ProviderViewModel();
                mdl.ProviderName = provider.GetType().Name;

                switch (provider)
                {
                    case ChainedConfigurationProvider chained:
                        continue;
                    case JsonConfigurationProvider j:
                        mdl.Source = j.Source.Path;
                        mdl.ConfigValues = GetProtectedData<JsonConfigurationProvider>(j);
                        break;
                    case EnvironmentVariablesConfigurationProvider e:
                        mdl.Source = "Environment Variables";
                        mdl.ConfigValues = GetProtectedData<EnvironmentVariablesConfigurationProvider>(e);
                        break;
                    case CommandLineConfigurationProvider cli:
                        mdl.Source = "Command line args";
                        mdl.ConfigValues = GetProtectedData<CommandLineConfigurationProvider>(cli);
                        break;
                    default:
                        break;
                }

                providerViewModels.Add(mdl);
            }
            return providerViewModels;
        }

        /// The config values are in the "Data" property of the configuration providers we are interested
        /// in. This is a protected property so dig it out with Reflection.
        /// IN CASE YOU ARE WONDERING, NO, THIS IS NOT HOW YOU SHOULD GET CONFIGURATION VALUES! This is
        /// just for demonstration.
        private Dictionary<string, string> GetProtectedData<T>(Object instance)
        {
            PropertyInfo pInfo = typeof(T).GetProperty("Data", BindingFlags.NonPublic | BindingFlags.Instance);

            if (pInfo != null)
            {
                var data = pInfo.GetValue(instance) as IDictionary<string, string>;
                return data.ToDictionary(d => d.Key, d => d.Value);
            }
            return null;
        }
    }
}
