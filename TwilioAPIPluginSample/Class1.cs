using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Twilio;
using System.ServiceModel;

namespace TwilioAPIPluginSample
{
    public class pluginClass: IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") &&
            context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];

                // Verify that the target entity represents an account.
                // If not, this plug-in was not registered correctly.
                if (entity.LogicalName != "account")
                    return;

                try
                {
                    //get initiating user (or impersonated user)
                    Guid userId = context.InitiatingUserId; //or context.UserId


                    //<snippetFollowupPlugin4>
                    // Obtain the organization service reference.
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(userId);
                    //</snippetFollowupPlugin4>


                    //retrieve user record
                    ColumnSet allFields = new ColumnSet() { AllColumns = true };
                    Entity user = service.Retrieve("systemuser", userId, allFields);

                    //get mobile phone number
                    string PhoneNumber = user["Mobile Phone"].ToString();

                    //Call sendText
                    sendText(PhoneNumber);
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in the FollowupPlugin plug-in.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("FollowupPlugin: {0}", ex.ToString());
                    throw;
                }


            }


        }


        public void sendText(String PhoneNumber)
        {

            var accountSid = "YOUR_SID_HERE"; // Your Account SID from www.twilio.com/console
            var authToken = "YOUR_AUTH_TOKEN_HERE";  // Your Auth Token from www.twilio.com/console

            var twilio = new TwilioRestClient(accountSid, authToken);
            var message = twilio.SendMessage(
                               "+15555555555", // From (Replace with your Twilio number)
                                PhoneNumber, // To (Replace with your phone number)
                                "Hello from C#"
                                );

        }
    }
}
