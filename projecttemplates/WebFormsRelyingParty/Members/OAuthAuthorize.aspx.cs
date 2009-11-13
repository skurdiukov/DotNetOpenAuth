﻿//-----------------------------------------------------------------------
// <copyright file="OAuthAuthorize.aspx.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty.Members {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.Messages;
	using WebFormsRelyingParty.Code;

	public partial class OAuthAuthorize : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack) {
				var pendingRequest = OAuthServiceProvider.PendingAuthorizationRequest;
				if (pendingRequest == null) {
					Response.Redirect("~/");
				}

				this.csrfCheck.Value = Utilities.SetCsrfCookie();
				this.consumerNameLabel.Text = HttpUtility.HtmlEncode(OAuthServiceProvider.PendingAuthorizationConsumer.Name);
				OAuth10ConsumerWarning.Visible = pendingRequest.IsUnsafeRequest;
			} else {
				Utilities.VerifyCsrfCookie(this.csrfCheck.Value);
			}
		}

		protected void yesButton_Click(object sender, EventArgs e) {
			outerMultiView.SetActiveView(authorizationGrantedView);

			var consumer = OAuthServiceProvider.PendingAuthorizationConsumer;
			var tokenManager = OAuthServiceProvider.ServiceProvider.TokenManager;
			var pendingRequest = OAuthServiceProvider.PendingAuthorizationRequest;
			ITokenContainingMessage requestTokenMessage = pendingRequest;
			var requestToken = tokenManager.GetRequestToken(requestTokenMessage.Token);

			OAuthServiceProvider.AuthorizePendingRequestToken();

			// The rest of this method only executes if we couldn't automatically
			// redirect to the consumer.
			if (pendingRequest.IsUnsafeRequest) {
				verifierMultiView.SetActiveView(noCallbackView);
			} else {
				verifierMultiView.SetActiveView(verificationCodeView);
				string verifier = ServiceProvider.CreateVerificationCode(consumer.VerificationCodeFormat, consumer.VerificationCodeLength);
				verificationCodeLabel.Text = verifier;
				requestToken.VerificationCode = verifier;
				tokenManager.UpdateToken(requestToken);
			}
		}

		protected void noButton_Click(object sender, EventArgs e) {
			outerMultiView.SetActiveView(authorizationDeniedView);
			OAuthServiceProvider.PendingAuthorizationRequest = null;
		}
	}
}
