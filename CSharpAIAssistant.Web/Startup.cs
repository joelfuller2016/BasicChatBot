using System;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using CSharpAIAssistant.BusinessLogic;

[assembly: OwinStartup(typeof(CSharpAIAssistant.Web.Startup))]

namespace CSharpAIAssistant.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            try
            {
                // Get application settings for OAuth configuration
                var settingsService = new ApplicationSettingsService();
                string googleClientId = settingsService.GetGoogleClientId();
                string googleClientSecret = settingsService.GetGoogleClientSecret();
                int sessionTimeoutMinutes = settingsService.GetSessionTimeoutMinutes();

                System.Diagnostics.Trace.WriteLine("Configuring OWIN authentication middleware...");

                // Configure Cookie Authentication
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                    LoginPath = new PathString("/Account/Login.aspx"),
                    LogoutPath = new PathString("/Account/Logout.aspx"),
                    CookieName = "CSharpAIAssistantAuth",
                    SlidingExpiration = true,
                    ExpireTimeSpan = TimeSpan.FromMinutes(sessionTimeoutMinutes),
                    CookieSecure = CookieSecureOption.SameAsRequest, // Use HTTPS in production
                    CookieHttpOnly = true
                });

                // Configure External Sign-In Cookie
                app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

                // Configure Google OAuth if settings are available and valid
                if (!string.IsNullOrEmpty(googleClientId) && 
                    !string.IsNullOrEmpty(googleClientSecret) &&
                    !googleClientId.Contains("YOUR_GOOGLE_CLIENT_ID") &&
                    !googleClientSecret.Contains("YOUR_GOOGLE_CLIENT_SECRET"))
                {
                    app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
                    {
                        ClientId = googleClientId,
                        ClientSecret = googleClientSecret,
                        Provider = new GoogleOAuth2AuthenticationProvider
                        {
                            OnAuthenticated = async context =>
                            {
                                try
                                {
                                    // Store additional claims from Google
                                    context.Identity.AddClaim(new System.Security.Claims.Claim("GoogleAccessToken", context.AccessToken));
                                    context.Identity.AddClaim(new System.Security.Claims.Claim("GoogleEmail", context.Email ?? ""));
                                    context.Identity.AddClaim(new System.Security.Claims.Claim("GoogleName", context.Name ?? ""));
                                    context.Identity.AddClaim(new System.Security.Claims.Claim("GoogleId", context.Id ?? ""));

                                    System.Diagnostics.Trace.WriteLine($"Google OAuth authenticated: Email={context.Email}, Name={context.Name}, ID={context.Id}");
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Trace.TraceError("Error in Google OAuth OnAuthenticated: {0}", ex.ToString());
                                }
                            },
                            OnApplyRedirect = context =>
                            {
                                // Custom redirect handling if needed
                                context.Response.Redirect(context.RedirectUri);
                            }
                        }
                    });

                    System.Diagnostics.Trace.WriteLine("Google OAuth configured successfully.");
                }
                else
                {
                    System.Diagnostics.Trace.TraceWarning("Google OAuth not configured - missing or invalid client ID/secret. Google sign-in will be disabled.");
                }

                System.Diagnostics.Trace.WriteLine("OWIN authentication middleware configuration completed.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error configuring OWIN startup: {0}", ex.ToString());
                
                // Log the error but don't prevent the application from starting
                // Instead, configure minimal authentication
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                    LoginPath = new PathString("/Account/Login.aspx"),
                    CookieName = "CSharpAIAssistantAuth",
                    SlidingExpiration = true,
                    ExpireTimeSpan = TimeSpan.FromMinutes(30)
                });
                
                app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
            }
        }
    }
}
