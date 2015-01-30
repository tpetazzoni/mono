﻿extern alias NewMonoSource;
extern alias MonoSecurity;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

using MonoSecurity::Mono.Security.Protocol.NewTls;
using NewSslPolicyErrors = NewMonoSource::System.Net.Security.SslPolicyErrors;
using SslProtocols = System.Security.Authentication.SslProtocols;
using EncryptionPolicy = NewMonoSource::System.Net.Security.EncryptionPolicy;
using MonoSslStreamFactory = NewMonoSource::Mono.Security.NewMonoSource.MonoSslStreamFactory;
using MonoSslStream = NewMonoSource::Mono.Security.NewMonoSource.MonoSslStream;

using SSCX = System.Security.Cryptography.X509Certificates;
using MX = MonoSecurity::Mono.Security.X509;

namespace Mono.Security.Instrumentation.Console
{
	using Framework;

	public class MonoClient : MonoConnection, IClient
	{
		new public IClientParameters Parameters {
			get { return (IClientParameters)base.Parameters; }
		}

		public MonoClient (ClientFactory factory, IPEndPoint endpoint, IClientParameters parameters)
			: base (factory, endpoint, parameters)
		{
		}

		protected override TlsSettings GetSettings ()
		{
			var settings = new TlsSettings ();
			var monoParams = Parameters as IMonoClientParameters;
			if (monoParams != null) {
				settings.ClientCertificateParameters = monoParams.ClientCertificateParameters;
				settings.Instrumentation = monoParams.ClientInstrumentation;
			}
			settings.RequestedCiphers = Parameters.ClientCiphers;
			return settings;
		}

		protected override MonoSslStream Start (Socket socket, TlsSettings settings)
		{
			Debug ("Connected.");

			var clientCerts = new X509Certificate2Collection ();
			if (Parameters.ClientCertificate != null)
				clientCerts.Add (Parameters.ClientCertificate.Certificate);

			var targetHost = "Hamiller-Tube.local";

			var stream = new NetworkStream (socket);
			return MonoSslStreamFactory.CreateClient (
				stream, false, RemoteValidationCallback, null, EncryptionPolicy.RequireEncryption, settings,
				targetHost, clientCerts, SslProtocols.Tls12, false);
		}

		bool RemoteValidationCallback (object sender, X509Certificate certificate, X509Chain chain, NewSslPolicyErrors errors)
		{
			return base.RemoteValidationCallback (sender, certificate, chain, (SslPolicyErrors)errors);
		}

		bool ClientCertValidationCallback (ClientCertificateParameters certParams, MX.X509Certificate certificate, MX.X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}
	}
}
